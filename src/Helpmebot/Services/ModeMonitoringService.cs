using System.Linq;
using System.Text.RegularExpressions;
using Helpmebot.IRC.Model;
using Helpmebot.Model.Interfaces;

namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;

    using Castle.Core.Logging;
    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.IRC.Messages;
    using Helpmebot.Model.ModeMonitoring;
    using Helpmebot.Services.Interfaces;
    
    public class ModeMonitoringService : IModeMonitoringService
    {
        /// <summary>
        /// Map of watch => report channels
        /// </summary>
        private readonly Dictionary<string, string> watchedChannels = new Dictionary<string, string>
                                                                          {
            { "##stwalkerster", "##stwalkerster-development" },
            { "#wikipedia-en-help", "#wikipedia-en-helpers" }
                                                                          };
        
        private readonly Dictionary<string, ChannelStatus> channelStatus = new Dictionary<string, ChannelStatus>();

        private readonly IIrcClient ircClient;
        private readonly ILogger logger;

        private readonly Dictionary<string, List<string>> banlist = new Dictionary<string, List<string>>();

        /// <summary>
        /// Regex for detecting a rejection for chanops by ChanServ
        /// </summary>
        private readonly Regex noChanopsPattern;

        public ModeMonitoringService(IIrcClient ircClient, ILogger logger)
        {
            this.ircClient = ircClient;
            this.logger = logger;

            var pattern = "you are not authorized to \\(de\\)op \x02" +
                             this.ircClient.Nickname.ToLower() + "\x02 on \x02(?<channel>#[^\\t ]+)\x02";

            this.noChanopsPattern = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            this.ircClient.ModeReceivedEvent += this.OnModeReceived;
            this.ircClient.JoinReceivedEvent += this.OnJoinReceived;
            this.ircClient.PartReceivedEvent += this.OnPartReceived;
            this.ircClient.ReceivedMessage += this.OnMessageReceived;
            this.logger.Info("Hooked into IRC events");
        }

        public void ResyncChannel(string channel)
        {
            this.logger.InfoFormat("Resyncing channel status for {0}", channel);
            // TODO: implement
        }

        #region Basic IRC Events

        private void OnPartReceived(object sender, JoinEventArgs e)
        {
            if (e.User.Nickname != this.ircClient.Nickname)
            {
                return;
            }
            
            lock (this.channelStatus)
            {
                var channelName = e.Channel.ToLowerInvariant();
                if (this.channelStatus.ContainsKey(channelName))
                {
                    this.logger.InfoFormat("Left channel {0}, ending tracking", channelName);
                    this.channelStatus.Remove(channelName);
                }
            }
        }

        private void OnNoticeReceived(MessageReceivedEventArgs e, IUser user)
        {
            var target = e.Message.Parameters.First();
            var message = e.Message.Parameters.Skip(1).First();

            this.logger.DebugFormat("Received notice from {0} for {1} with content: {2}", user, target, message);

            if (user.Nickname == "ChanServ")
            {
                var match = this.noChanopsPattern.Match(message);
                if (match.Success)
                {
                    // oops, we don't have ops here.
                    string channel = match.Groups["channel"].Value;
                    this.logger.ErrorFormat("Cannot obtain ops for channel {0}, skipping", channel);

                    this.ircClient.SendMessage(
                        this.watchedChannels[channel],
                        string.Format(
                            "Unable to obtain ops from ChanServ in {0}; I won't be monitoring this channel for ban exemptions",
                            channel));

                    lock (this.channelStatus)
                    {
                        this.watchedChannels.Remove(channel.ToLowerInvariant());
                        this.channelStatus.Remove(channel.ToLowerInvariant());
                    }
                }
            }
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Command == "NOTICE")
            {
                IUser user = IrcUser.FromPrefix(e.Message.Prefix);

                this.OnNoticeReceived(e, user);
            }

            if (e.Message.Command == Numerics.BanListEntry)
            {
                var parameters = e.Message.Parameters.ToList();
                this.ProcessBanListEntry(parameters[2], parameters[1], "b");
            }

            if (e.Message.Command == Numerics.QuietListEntry)
            {
                var parameters = e.Message.Parameters.ToList();
                this.ProcessBanListEntry(parameters[3], parameters[1], "q");
            }

            if (e.Message.Command == Numerics.ExemptListEntry)
            {
                var parameters = e.Message.Parameters.ToList();
                this.ProcessBanListEntry(parameters[2], parameters[1], "e");
            }

            if (e.Message.Command == Numerics.BanListEnd)
            {
                var parameters = e.Message.Parameters.ToList();
                this.ProcessBanListEnd(parameters[1], "b");
            }

            if (e.Message.Command == Numerics.QuietListEnd)
            {
                var parameters = e.Message.Parameters.ToList();
                this.ProcessBanListEnd(parameters[1], "q");
            }

            if (e.Message.Command == Numerics.ExemptListEnd)
            {
                var parameters = e.Message.Parameters.ToList();
                this.ProcessBanListEnd(parameters[1], "e");
            }
        }

        private void OnModeReceived(object sender, ModeEventArgs e)
        {
            var channel = e.Target.ToLowerInvariant();

            if (this.watchedChannels.ContainsKey(channel))
            {
                var changes = ModeChanges.FromChangeList(e.Changes);

                var triggerBotOpped = false;
                var triggerBotDeOpped = false;

                this.logger.DebugFormat("Mode change seen on {0}: {1}", channel, string.Join(" ", e.Changes));

                lock (this.channelStatus)
                {
                    if (changes.Ops.Contains(this.ircClient.Nickname))
                    {
                        triggerBotOpped = true;
                        changes.Ops.Remove(this.ircClient.Nickname);
                    }

                    if (changes.Deops.Contains(this.ircClient.Nickname))
                    {
                        triggerBotDeOpped = true;
                        changes.Deops.Remove(this.ircClient.Nickname);
                    }

                    this.SyncChangesToChannel(this.channelStatus[channel], changes);
                }

                if (triggerBotOpped)
                {
                    this.logger.InfoFormat("Bot opped on {0}", channel);
                    this.OnBotOpped(channel);
                }

                if (triggerBotDeOpped)
                {
                    this.logger.InfoFormat("Bot deopped on {0}", channel);
                    this.OnBotDeOpped(channel);
                }

                if (!changes.IsEmpty())
                {
                    this.logger.InfoFormat("Mode change on {0}", channel);
                    this.OnModeChange(channel);
                }
                
            }

        }

        private void OnJoinReceived(object sender, JoinEventArgs e)
        {
            // is this me?
            if (e.User.Nickname == this.ircClient.Nickname)
            {
                this.logger.Debug("Seen self-join");

                var channel = e.Channel.ToLowerInvariant();

                // is this one of our channels?
                if (this.watchedChannels.ContainsKey(channel))
                {
                    this.logger.DebugFormat("Self-join monitored channel {0}", channel);

                    lock (this.channelStatus)
                    {
                        if (this.channelStatus.ContainsKey(channel))
                        {
                            this.channelStatus.Remove(channel);
                        }

                        this.channelStatus.Add(channel, new ChannelStatus());
                    }

                    this.logger.InfoFormat("Requesting ban/quiet lists for {0}", channel);
                    this.ircClient.Mode(channel, "b");
                    this.ircClient.Mode(channel, "q");
                }

            }
        }

        #endregion

        #region Banlist stuff

        private void ProcessBanListEntry(string ban, string channel, string type)
        {
            var banlistKey = string.Format("{0}/{1}", type, channel);

            if (!this.banlist.ContainsKey(banlistKey))
            {
                this.banlist.Add(banlistKey, new List<string>());
            }

            this.banlist[banlistKey].Add(ban);
        }

        private void ProcessBanListEnd(string channel, string type)
        {
            var banlistKey = string.Format("{0}/{1}", type, channel);

            this.logger.InfoFormat("Banlist for {0} downloaded successfully", banlistKey);

            // default to an empty list in case there isn't anything.
            List<string> banlist = new List<string>();
            if (this.banlist.ContainsKey(banlistKey))
            {
                banlist = this.banlist[banlistKey];
                this.banlist.Remove(banlistKey);
            }

            bool readyToAnalyse;

            lock (this.channelStatus)
            {
                switch (type)
                {
                    case "b":
                        this.channelStatus[channel].Bans.Clear();
                        this.channelStatus[channel].Bans.AddRange(banlist);
                        this.channelStatus[channel].BanListDownloaded = true;
                        break;
                    case "q":
                        this.channelStatus[channel].Quiets.Clear();
                        this.channelStatus[channel].Quiets.AddRange(banlist);
                        this.channelStatus[channel].QuietListDownloaded = true;
                        break;
                    case "e":
                        this.channelStatus[channel].Exempts.Clear();
                        this.channelStatus[channel].Exempts.AddRange(banlist);
                        this.channelStatus[channel].ExemptListDownloaded = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }

                readyToAnalyse = this.channelStatus[channel].BanListDownloaded
                                 && this.channelStatus[channel].QuietListDownloaded;
            }

            if (readyToAnalyse)
            {
                if (type == "e")
                {
                    this.FixChannel(channel);
                }
                else
                {
                    this.CheckRestrictivity(channel);
                }
            }
        }

        #endregion

        #region Mode changes

        private void OnBotOpped(string channel)
        {
            lock (this.channelStatus)
            {
                this.channelStatus[channel].BotOpsHeld = true;
            }

            // request exemption lists
            this.ircClient.Mode(channel, "e");
            this.logger.DebugFormat("Requested exemption lists for {0}", channel);
        }

        private void OnBotDeOpped(string channel)
        {
            lock (this.channelStatus)
            {
                this.channelStatus[channel].BotOpsHeld = false;
            }

            this.CheckRestrictivity(channel);
        }

        private void OnModeChange(string channel)
        {
            this.CheckRestrictivity(channel);
        }

        #endregion

        #region Channel fixing

        private void CheckRestrictivity(string channel)
        {
            bool needsChanges = false;
            bool needsOps = false;

            bool hasOps = false;

            lock (this.channelStatus)
            {
                var status = this.channelStatus[channel];

                if (status.RegisteredOnly || status.Moderated)
                {
                    needsChanges = true;
                }

                if (status.Bans.Contains("$~a") || status.Quiets.Contains("$~a"))
                {
                    needsOps = true;
                }

                hasOps = status.BotOpsHeld;
            }

            needsOps |= needsChanges;

            if (hasOps && !needsOps)
            {
                // drop ops
                this.logger.DebugFormat("Detected we have ops, and don't need it. Dropping for {0}", channel);
                this.ircClient.Mode(channel, "-o " + this.ircClient.Nickname);
            }

            if (needsOps && !hasOps)
            {
                this.logger.DebugFormat("Detected we need ops, and don't have it. Requesting on {0}", channel);
                this.ircClient.SendMessage("ChanServ", "op " + channel);
            }
            
        }

        private void FixChannel(string channel)
        {
            
        }

        #endregion

        /// <summary>
        /// Updates the channel status object with the changes. Must be called inside a lock() only.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="modeChanges"></param>
        private void SyncChangesToChannel(ChannelStatus status, ModeChanges modeChanges)
        {
            if (modeChanges.Ops.Contains(this.ircClient.Nickname))
            {
                status.BotOpsHeld = true;
                status.BotOpsRequested = false;
            }

            if (modeChanges.Deops.Contains(this.ircClient.Nickname))
            {
                status.BotOpsHeld = false;
            }

            if (modeChanges.ReducedModeration.HasValue)
            {
                status.ReducedModeration = modeChanges.ReducedModeration.Value;
            }

            if (modeChanges.Moderated.HasValue)
            {
                status.Moderated = modeChanges.Moderated.Value;
            }

            if (modeChanges.RegisteredOnly.HasValue)
            {
                status.RegisteredOnly = modeChanges.RegisteredOnly.Value;
            }

            modeChanges.Unbans.ForEach(x => status.Bans.Remove(x));
            modeChanges.Unquiets.ForEach(x => status.Quiets.Remove(x));
            modeChanges.Unexempts.ForEach(x => status.Exempts.Remove(x));

            modeChanges.Bans.ForEach(x => status.Bans.Add(x));
            modeChanges.Quiets.ForEach(x => status.Quiets.Add(x));
            modeChanges.Exempts.ForEach(x => status.Exempts.Add(x));
        }
    }
}