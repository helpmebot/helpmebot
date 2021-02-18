namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Timers;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Commands.ChannelManagement;
    using Helpmebot.ChannelServices.ExtensionMethods;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Messages;
    using Stwalkerster.IrcClient.Model.Interfaces;
    using Timer = System.Timers.Timer;

    public class TrollMonitorService : ITrollMonitoringService
    {
        private readonly IIrcClient client;
        private readonly ILogger logger;
        private readonly IModeMonitoringService modeMonitoringService;
        private readonly ICommandParser commandParser;
        private List<IPNetwork> networks;

        private Dictionary<IUser, uint> trackedUsers = new Dictionary<IUser, uint>();
        private Regex emojiRegex;
        private Regex badWordRegex;
        private Regex reallyBadWordRegex;

        private IUser banProposal;

        #if DEBUG
            private string targetChannel = "##stwalkerster";
            private string publicAlertChannel = "##stwalkerster-development2";
            private string[] privateAlertTargets = {"stwalkerster"};
            private string banTracker = "stwalkerster";
        #else
            private string targetChannel = "#wikipedia-en-help";
            private string publicAlertChannel = "#wikipedia-en-helpers";
            private string[] privateAlertTargets = {"stwalkerster" , "Waggie"};
            private string banTracker = "eir";
        #endif        

        private Timer banProposalTimer = new Timer(60000);
        

        public TrollMonitorService(IIrcClient client, ILogger logger, IModeMonitoringService modeMonitoringService, ICommandParser commandParser)
        {
            this.client = client;
            this.logger = logger;
            this.modeMonitoringService = modeMonitoringService;
            this.commandParser = commandParser;

            this.networks = new List<IPNetwork>
            {
                IPNetwork.Parse("103.139.56.0/23"), // Avjr
                IPNetwork.Parse("110.235.224.0/20"), // Excitel
                
                // Reliance Jio
                IPNetwork.Parse("45.123.16.0/22"),
                IPNetwork.Parse("47.8.0.0/15"),
                IPNetwork.Parse("47.11.0.0/16"),
                IPNetwork.Parse("47.15.0.0/16"),
                IPNetwork.Parse("47.29.0.0/16"),
                IPNetwork.Parse("47.30.0.0/15"),
                IPNetwork.Parse("47.247.0.0/16"),
                IPNetwork.Parse("49.32.0.0/13"),
                IPNetwork.Parse("49.40.0.0/14"),
                IPNetwork.Parse("49.44.48.0/20"),
                IPNetwork.Parse("49.44.64.0/18"),
                IPNetwork.Parse("49.44.128.0/17"),
                IPNetwork.Parse("49.45.0.0/16"),
                IPNetwork.Parse("49.46.0.0/15"),
                IPNetwork.Parse("103.63.128.0/22"),
                IPNetwork.Parse("115.240.0.0/13"),
                IPNetwork.Parse("132.154.0.0/16"),
                IPNetwork.Parse("136.232.0.0/15"),
                IPNetwork.Parse("137.97.0.0/16"),
                IPNetwork.Parse("139.167.0.0/16"),
                IPNetwork.Parse("152.56.0.0/14"),
                IPNetwork.Parse("157.32.0.0/12"),
                IPNetwork.Parse("157.48.0.0/14"),
                IPNetwork.Parse("169.149.0.0/16"),
                IPNetwork.Parse("205.253.0.0/16"),
                
                // M247 / NordVPN
                IPNetwork.Parse("37.120.221.0/24")
            };

            this.emojiRegex = new Regex("(\\u00a9|\\u00ae|[\\u2000-\\u3300]|\\ud83c[\\ud000-\\udfff]|\\ud83d[\\ud000-\\udfff]|\\ud83e[\\ud000-\\udfff])", RegexOptions.IgnoreCase);
            
            this.badWordRegex = new Regex("(cock|pussy|fuck|babes|dick|ur mom|belle|delphine|uwu|shit)", RegexOptions.IgnoreCase);
            this.reallyBadWordRegex = new Regex("(hard core|hardcore|cunt|nigger|niggers|jews|9/11|aids|blowjob|cumshot|suk mai dik)", RegexOptions.IgnoreCase);

            this.banProposalTimer.Enabled = false;
            this.banProposalTimer.AutoReset = false;
            this.banProposalTimer.Elapsed += BanProposalTimerOnElapsed;
        }

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
                        ircClient.Send(new Message("REMOVE", new[] {this.targetChannel, proposal.Nickname}));
                        
                        // allow time for eir to message us
                        Thread.Sleep(1000);
                        ircClient.SendMessage(this.banTracker, $"~1d Requested by {enactingUser} following bot-initiated proposal");
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

            var badWordMatch = this.badWordRegex.Match(e.Message);
            var reallyBadWordMatch = this.reallyBadWordRegex.Match(e.Message);
            
            if (!this.trackedUsers.ContainsKey(e.User))
            {
                var channelUser = this.client.Channels[this.targetChannel].Users[e.User.Nickname];
                if ((badWordMatch.Success || reallyBadWordMatch.Success ) && !channelUser.Voice)
                {
                    // add to tracking anyway.
                    this.trackedUsers.Add(e.User, 0);
                    this.SendIrcPrivateAlert($"UNTRACKED unvoiced user {e.User} added to tracking due to badword filter");

                } else {
                    // don't care about non-tracked users
                    return;
                }
            }

            try
            {
                if (this.client.Channels[this.targetChannel].Users[e.User.Nickname].Voice)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("Exception encountered checking user's voice status", ex);
            }

            var emojiMatch = this.emojiRegex.Match(e.Message);
            if (emojiMatch.Success)
            {
                this.SendIrcPrivateAlert($"Tracked user {e.User} in -en-help SENT EMOJI");
                this.trackedUsers[e.User]++;
            }

            if (reallyBadWordMatch.Success)
            {
                this.SendIrcPrivateAlert($"Tracked user {e.User} in -en-help SENT REALLYBADWORD");
                this.trackedUsers[e.User] += 3;
            }
            
            if (badWordMatch.Success)
            {
                this.SendIrcPrivateAlert($"Tracked user {e.User} in -en-help SENT BADWORD");
                this.trackedUsers[e.User]++;
            }

            if (this.trackedUsers[e.User] >= 3) 
            {
                this.SendIrcPrivateAlert($"Tracked user {e.User} in -en-help matched {this.trackedUsers[e.User]} alerts  *** PROPOSED BAN ***  Use !enact within the next 60 seconds to apply.");
                this.SendIrcAlert($"Tracked user {e.User} in -en-help matched {this.trackedUsers[e.User]} alerts");
                this.banProposal = e.User;
                try
                {
                    this.commandParser.RegisterCommand("enact", typeof(EnactBanCommand));
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
            foreach (var target in privateAlertTargets)
            {
                this.client.SendMessage(target, message);    
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
                this.AddTracking(e.User);
            }
        }
        #endregion

        public void ForceAddTracking(IUser user, object sender)
        {
            this.logger.DebugFormat("Tracking {0} per request of other service", user);
            try
            {
                this.trackedUsers.Add(user, 0);
            }
            catch
            {
            }

            this.SendIrcPrivateAlert($"Tracked user {user} in -en-help per request of {sender}");
        }
        
        private void AddTracking(IUser user)
        {
            var address = user.GetIpAddress();

            // no IP detected, don't track
            if (address == null)
            {
                this.logger.DebugFormat("Not tracking {0}, no IP detected", user);
                return;
            }

            if (this.networks.Any(x => x.Contains(address)))
            {
                this.logger.DebugFormat("Tracking {0}, in targeted ranges", user);
                this.trackedUsers.Add(user, 0);
            
                this.SendIrcPrivateAlert($"Tracked user {user} in -en-help from target ranges");
                return;
            }
        }

        private void RemoveTracking(IUser user)
        {
            this.trackedUsers.Remove(user);
        }
        
        public void Start()
        {
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