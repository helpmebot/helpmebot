namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Timers;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Commands.ChannelManagement;
    using Helpmebot.ChannelServices.ExtensionMethods;
    using Helpmebot.ChannelServices.Model;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.Configuration;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Messages;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;
    using Timer = System.Timers.Timer;

    public class TrollMonitorService : ITrollMonitoringService
    {
        private readonly IIrcClient client;
        private readonly ILogger logger;
        private readonly IModeMonitoringService modeMonitoringService;
        private readonly ICommandParser commandParser;
        private readonly BotConfiguration config;
        private List<IPNetwork> networks;

        private Dictionary<IUser, TrackedUser> trackedUsers = new Dictionary<IUser, TrackedUser>();
        private Regex emojiRegex;
        private readonly Regex badWordRegex;
        private readonly Regex reallyBadWordRegex;
        private readonly Regex pasteRegex;
        private readonly Regex instaQuietRegex;
        private readonly Regex firstMessageQuietRegex;

        private IUser banProposal;

        #if DEBUG
            private string targetChannel = "##stwalkerster-development2";
            private string publicAlertChannel = "##stwalkerster-development";
            private string[] privateAlertTargets = {"stwalkerster"};
            private string banTracker = "##stwalkerster-development3";
        #else
            private string targetChannel = "#wikipedia-en-help";
            private string publicAlertChannel = "#wikipedia-en-helpers";
            private string[] privateAlertTargets = {"stwalkerster", "Waggie"};
            private string banTracker = "litharge";
        #endif        

        private Timer banProposalTimer = new Timer(60000);

        public TrollMonitorService(
            IIrcClient client,
            ILogger logger,
            IModeMonitoringService modeMonitoringService,
            ICommandParser commandParser,
            BotConfiguration config)
        {
            this.client = client;
            this.logger = logger;
            this.modeMonitoringService = modeMonitoringService;
            this.commandParser = commandParser;
            this.config = config;

            this.networks = new List<IPNetwork>
            {
                // IPNetwork.Parse("103.139.56.0/23"), // Avjr
                // IPNetwork.Parse("110.235.224.0/20"), // Excitel
                //
                // // Reliance Jio
                // IPNetwork.Parse("45.123.16.0/22"),
                // IPNetwork.Parse("47.8.0.0/15"),
                // IPNetwork.Parse("47.11.0.0/16"),
                // IPNetwork.Parse("47.15.0.0/16"),
                // IPNetwork.Parse("47.29.0.0/16"),
                // IPNetwork.Parse("47.30.0.0/15"),
                // IPNetwork.Parse("47.247.0.0/16"),
                // IPNetwork.Parse("49.32.0.0/13"),
                // IPNetwork.Parse("49.40.0.0/14"),
                // IPNetwork.Parse("49.44.48.0/20"),
                // IPNetwork.Parse("49.44.64.0/18"),
                // IPNetwork.Parse("49.44.128.0/17"),
                // IPNetwork.Parse("49.45.0.0/16"),
                // IPNetwork.Parse("49.46.0.0/15"),
                // IPNetwork.Parse("103.63.128.0/22"),
                // IPNetwork.Parse("115.240.0.0/13"),
                // IPNetwork.Parse("132.154.0.0/16"),
                // IPNetwork.Parse("136.232.0.0/15"),
                // IPNetwork.Parse("137.97.0.0/16"),
                // IPNetwork.Parse("139.167.0.0/16"),
                // IPNetwork.Parse("152.56.0.0/14"),
                // IPNetwork.Parse("157.32.0.0/12"),
                // IPNetwork.Parse("157.48.0.0/14"),
                // IPNetwork.Parse("169.149.0.0/16"),
                // IPNetwork.Parse("205.253.0.0/16"),
                //
                // // M247 / NordVPN
                // IPNetwork.Parse("37.120.221.0/24")
            };

            this.emojiRegex = new Regex("(\\u00a9|\\u00ae|[\\u2000-\\u3300]|\\ud83c[\\ud000-\\udfff]|\\ud83d[\\ud000-\\udfff]|\\ud83e[\\ud000-\\udfff])", RegexOptions.IgnoreCase);

            this.badWordRegex = new Regex("(cock|pussy|fuck|babes|dick|ur mom|belle|delphine|uwu|shit)", RegexOptions.IgnoreCase);
            this.reallyBadWordRegex = new Regex("(hard core|hardcore|cunt|nigger|niggers|jews|9/11|aids|blowjob|cumshot|suk mai dik|skiyomi|yamlafuck|deepfuckfuck|pooyo)", RegexOptions.IgnoreCase);
            this.instaQuietRegex = new Regex("(yamlafuck pooyo and deepfuckfuck|free skiyomi and other ltas|hope you all die and kill yourself)", RegexOptions.IgnoreCase);
            this.firstMessageQuietRegex = new Regex("^\\s*(fuck you|hi fuckers)\\s*$", RegexOptions.IgnoreCase);

            this.pasteRegex = new Regex("^Uploaded file: (?<url>https://uploads\\.kiwiirc\\.com/files/[a-z0-9]{32}/pasted\\.txt)", RegexOptions.IgnoreCase);

            this.banProposalTimer.Enabled = false;
            this.banProposalTimer.AutoReset = false;
            this.banProposalTimer.Elapsed += this.BanProposalTimerOnElapsed;
        }

        private IJoinMessageService JoinMessageService => this.BlockMonitoringService.JoinMessageService;
        public IBlockMonitoringService BlockMonitoringService { get; set; }
        
        private void BanProposalTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            this.commandParser.UnregisterCommand("enact");
            this.banProposal = null;
        }

        public void EnactBan(IUser enactingUser)
        {
            if (this.banProposal != null)
            {
                // copy locally due to threading race conditions
                var proposal = this.banProposal;
                
                this.modeMonitoringService.PerformAsOperator(this.targetChannel,
                    ircClient =>
                    {
                        ircClient.Mode(this.targetChannel, $"+b *!*@{proposal.Hostname}");
                        ircClient.Send(new Message("KICK", new[] {this.targetChannel, proposal.Nickname}));
                        
                        // allow time for eir to message us
                        Thread.Sleep(1000);
                        ircClient.SendMessage(this.banTracker, $"1d Requested by {enactingUser} following bot-initiated proposal");
                    });
                
                this.banProposalTimer.Stop();
                this.BanProposalTimerOnElapsed(this, null);
            }
        }

        private void ClientOnReceivedMessage(object sender, MessageReceivedEventArgs e)
        {
            if (e.Target != this.targetChannel)
            {
                // don't care about other channels.
                return;
            }
            
            var pasteMatch = this.pasteRegex.Match(e.Message);

            var badWordMatch = this.badWordRegex.Match(e.Message);
            var reallyBadWordMatch = this.reallyBadWordRegex.Match(e.Message);
            var instaQuietMatch = this.instaQuietRegex.Match(e.Message);
            var firstMessageQuietMatch = this.firstMessageQuietRegex.Match(e.Message);
            
            if (pasteMatch.Success)
            {
                var url = pasteMatch.Groups["url"].Value;
                    
                var hwr = (HttpWebRequest)WebRequest.Create(url);
                hwr.UserAgent = this.config.UserAgent;
                hwr.Method = "GET";

                var memoryStream = new MemoryStream();
                using (var resp = (HttpWebResponse) hwr.GetResponse())
                {               
                    var responseStream = resp.GetResponseStream();

                    if (responseStream != null)
                    {
                        responseStream.CopyTo(memoryStream);
                        resp.Close();
                    }
                }

                memoryStream.Position = 0;
                var newMessage = new StreamReader(memoryStream).ReadToEnd();
                
                badWordMatch = this.badWordRegex.Match(newMessage);
                reallyBadWordMatch = this.reallyBadWordRegex.Match(newMessage);
                instaQuietMatch = this.instaQuietRegex.Match(newMessage);
                firstMessageQuietMatch = this.firstMessageQuietRegex.Match(newMessage);
            }
            
            // add to tracking
            if (!this.trackedUsers.ContainsKey(e.User))
            {
                var channelUser = this.client.Channels[this.targetChannel].Users[e.User.Nickname];
                if ((badWordMatch.Success || reallyBadWordMatch.Success || instaQuietMatch.Success || firstMessageQuietMatch.Success ) && !channelUser.Voice)
                {
                    // add to tracking anyway.
                    this.trackedUsers.Add(e.User, new TrackedUser(0, e.User));
                    this.logger.InfoFormat($"UNTRACKED unvoiced user {e.User} added to tracking due to filter hit");

                } else {
                    // don't care about non-tracked users who haven't hit the alert.
                    return;
                }
            }

            try
            {
                if (this.client.Channels[this.targetChannel].Users[e.User.Nickname].Voice)
                {
                    this.trackedUsers[e.User].CheckFirstMessage = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("Exception encountered checking user's voice status", ex);
            }

            // var emojiMatch = this.emojiRegex.Match(e.Message);
            // if (emojiMatch.Success)
            // {
            //     this.SendIrcPrivateAlert($"Tracked user {e.User} in {e.Target} SENT EMOJI");
            //     this.trackedUsers[e.User]++;
            // }

            var banSet = false;

            if (instaQuietMatch.Success)
            {
                this.logger.InfoFormat($"Tracked user {e.User} in {e.Target} automatically banned due to expression match");
                
                this.modeMonitoringService.PerformAsOperator(
                    this.targetChannel,
                    ircClient =>
                    {
                        ircClient.PrioritySend(
                            new Message(
                                "MODE",
                                new[] { this.targetChannel, "+bzo", $"*!*@{e.User.Hostname}", "ozone" }));
                        
                        if (this.JoinMessageService != null)
                        {
                            this.JoinMessageService.RemoveExemption(this.targetChannel, e.User, true);
                        }
                        else
                        {
                            this.logger.Error("Could not check for/remove exemptions set by JMS.");
                        }

                        Thread.Sleep(1000);
                        ircClient.SendMessage(this.banTracker, $"2w Applied automatically by bot following match expression hit: LTA (skiyomi).");
                    },
                    true);

                banSet = true;
                
                this.SendIrcPrivateAlert($"Tracked user {e.User} in {e.Target} SENT INSTAQUIET WORD, and was banned.");

                this.trackedUsers[e.User].Score += 1000;
                this.logger.DebugFormat($"Tracked user {e.User} now has score {this.trackedUsers[e.User].Score}");
            } 
            else if (firstMessageQuietMatch.Success && this.trackedUsers[e.User].CheckFirstMessage)
            {
                this.logger.InfoFormat($"Tracked user {e.User} in {e.Target} automatically quieted due to expression match on first message");

                this.modeMonitoringService.PerformAsOperator(
                    this.targetChannel,
                    ircClient =>
                    {
                        ircClient.PrioritySend(
                            new Message(
                                "MODE",
                                new[] { this.targetChannel, "+qzoo", $"*!*@{e.User.Hostname}", "ozone", "stw" }));

                        if (this.JoinMessageService != null)
                        {
                            this.JoinMessageService.RemoveExemption(this.targetChannel, e.User, true);
                        }
                        else
                        {
                            this.logger.Error("Could not check for/remove exemptions set by JMS.");
                        }

                        Thread.Sleep(1000);
                        ircClient.SendMessage(this.banTracker, $"1h Applied automatically by bot following match expression hit on first message.");
                    },
                    true);
                 
                this.SendIrcPrivateAlert($"Tracked user {e.User} in {e.Target} matched quiet-on-first-message filter, and was quieted.");

                this.trackedUsers[e.User].Score += 100;
                this.logger.DebugFormat($"Tracked user {e.User} now has score {this.trackedUsers[e.User].Score}");
            }
            else if (reallyBadWordMatch.Success)
            {
                this.trackedUsers[e.User].Score += 3;
                this.logger.DebugFormat($"Tracked user {e.User} now has score {this.trackedUsers[e.User].Score}");

                if (this.trackedUsers[e.User].Score > 0)
                {
                    this.SendIrcPrivateAlert($"Tracked user {e.User} in {e.Target} SENT REALLYBADWORD");
                    this.logger.InfoFormat($"Tracked user {e.User} in {e.Target} SENT REALLYBADWORD");
                }

            } 
            else if (badWordMatch.Success)
            {
                this.trackedUsers[e.User].Score++;
                this.logger.DebugFormat($"Tracked user {e.User} now has score {this.trackedUsers[e.User].Score}");
                
                if (this.trackedUsers[e.User].Score > 0)
                {
                    this.SendIrcPrivateAlert($"Tracked user {e.User} in {e.Target} SENT BADWORD");
                    this.logger.InfoFormat($"Tracked user {e.User} in {e.Target} SENT BADWORD");
                }
            }
            
            // we've now received a message, so don't check this again.
            this.trackedUsers[e.User].CheckFirstMessage = false;

            if (this.trackedUsers[e.User].Score >= 3 && !banSet) 
            {
                this.SendIrcPrivateAlert($"Tracked user {e.User} in {e.Target} has score={this.trackedUsers[e.User].Score}  *** PROPOSED BAN ***  Use !enact within the next 60 seconds to apply.");
                this.SendIrcAlert($"Tracked user {e.User} in {e.Target} has score={this.trackedUsers[e.User].Score}");
                this.logger.InfoFormat($"Tracked user {e.User} in {e.Target} matched {this.trackedUsers[e.User].Score} alerts; registering ban proposal");

                this.banProposal = e.User;
                try
                {
                    if (this.banProposalTimer.Enabled)
                    {
                        this.banProposalTimer.Stop();
                    }
                    else
                    {
                        this.commandParser.RegisterCommand("enact", typeof(EnactBanCommand));
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Error("Could not register command, is it already registered?");
                }

                this.banProposalTimer.Start();
            }
        }

        private void SendIrcPrivateAlert(string message)
        {
            foreach (var target in this.privateAlertTargets)
            {
                if (target.StartsWith("#"))
                {
                    this.client.SendMessage(target, message);
                }
                else
                {
                    foreach (var ircUserKvp in this.client.UserCache.Where(x => x.Value.Account == target))
                    {
                        this.client.SendMessage(ircUserKvp.Value.Nickname, message);
                    }
                }
            }
        }

        private void SendIrcAlert(string message)
        {
            this.client.SendNotice(this.publicAlertChannel, message);
        }

        #region tracking events
        private void IrcQuitReceived(object sender, QuitEventArgs e)
        {
            this.RemoveTracking(e.User);
        }

        private void IrcKickReceived(object sender, KickEventArgs e)
        {
            if (e.Channel == this.targetChannel)
            {
                this.RemoveTracking(e.KickedUser);
            }
        }

        private void IrcPartReceived(object sender, JoinEventArgs e)
        {
            if (e.Channel == this.targetChannel)
            {
                this.RemoveTracking(e.User);
            }
        }

        private void IrcJoinReceived(object sender, JoinEventArgs e)
        {
            if (e.Channel == this.targetChannel)
            {
                this.AddNetworkTracking(e.User);
            }
        }
        #endregion

        public void ForceAddTracking(IUser user, object sender)
        {
            this.logger.DebugFormat("Tracking {0} per request of other service", user);
            if (this.trackedUsers.ContainsKey(user))
            {
                // already tracked
                return;
            }
            
            try
            {
                this.trackedUsers.Add(user, new TrackedUser(0, user) { CheckFirstMessage = true });
                this.SendIrcPrivateAlert($"Tracked user {user} in -en-help per request of {sender}");
            }
            catch(Exception ex)
            {
                this.logger.Error("Could not add user to tracking", ex);
            }
        }
        
        private void AddNetworkTracking(IUser user)
        {
            IPAddress address;
            try
            {
                address = user.GetIpAddress();
            }   
            catch (Exception ex)
            {
                this.logger.DebugFormat(ex, "Not tracking {0}, no IP detected", user);
                return;
            }

            // no IP detected, don't track
            if (address == null)
            {
                this.logger.DebugFormat("Not tracking {0}, no IP detected", user);
                return;
            }

            if (this.networks.Any(x => x.Contains(address)))
            {
                this.logger.DebugFormat("Tracking {0}, in targeted ranges", user);
                this.trackedUsers.Add(user, new TrackedUser(0, user));
            
                this.SendIrcPrivateAlert($"Tracked user {user} in -en-help from target ranges");
                return;
            }
        }

        public void SetScore(IUser user, int score, bool checkFirstMessage)
        {
            if (!this.trackedUsers.ContainsKey(user))
            {
                this.trackedUsers.Add(user, new TrackedUser(score, user) { CheckFirstMessage = checkFirstMessage });
            }
            else
            {
                this.trackedUsers[user].Score = score;
                this.trackedUsers[user].CheckFirstMessage = checkFirstMessage;
            }
        }
        
        public IUser SetScore(string nickname, int score, bool checkFirstMessage)
        {
            IrcChannelUser channelUser;

            if (!this.client.Channels[this.targetChannel].Users.TryGetValue(nickname, out channelUser))
            {
                return null;
            }

            var user = channelUser.User;
            
            this.SetScore(user, score, checkFirstMessage);
            return user;
        }

        private void RemoveTracking(IUser user)
        {
            this.trackedUsers.Remove(user);
        }
        
        public void Start()
        {
            // disabled for now, pending configuration.
            
            this.client.JoinReceivedEvent += this.IrcJoinReceived;
            this.client.ReceivedMessage += this.ClientOnReceivedMessage;
            this.client.PartReceivedEvent += this.IrcPartReceived;
            this.client.KickReceivedEvent += this.IrcKickReceived;
            this.client.QuitReceivedEvent += this.IrcQuitReceived;
            this.logger.Debug("Troll monitoring startup");
        }

        public void Stop()
        {
            this.logger.Debug("Troll monitoring shutdown");
            this.client.JoinReceivedEvent -= this.IrcJoinReceived;
            this.client.ReceivedMessage -= this.ClientOnReceivedMessage;
            this.client.PartReceivedEvent -= this.IrcPartReceived;
            this.client.KickReceivedEvent -= this.IrcKickReceived;
            this.client.QuitReceivedEvent -= this.IrcQuitReceived;
        }
        
    }
}
