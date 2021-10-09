namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Model;
    using Helpmebot.ChannelServices.Model.ModeMonitoring;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.Configuration;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Messages;

    public class ModeMonitoringService : IModeMonitoringService
    {
        /// <summary>
        /// Map of watch => report channels, loaded from config.
        /// </summary>
        private readonly IDictionary<string, string> watchedChannels;
        private readonly IDictionary<string, ChannelStatus> channelStatus = new Dictionary<string, ChannelStatus>();
        private readonly IDictionary<string, List<string>> banlist = new Dictionary<string, List<string>>();
        private readonly IIrcClient ircClient;
        private readonly ILogger logger;
        private readonly IDictionary<string, IChannelOperator> onOpTaskList = new Dictionary<string, IChannelOperator>();

        /// <summary>
        /// Regex for detecting a rejection for chanops by ChanServ
        /// </summary>
        private readonly Regex noChanopsPattern;

        public ModeMonitoringService(IIrcClient ircClient, ILogger logger, ModeMonitorConfiguration modeMonitorConfiguration)
        {
            this.ircClient = ircClient;
            this.logger = logger;
            this.watchedChannels = modeMonitorConfiguration.ChannelMap;

            if (!this.watchedChannels.Any())
            {
                this.logger.Warn("No channels configured for mode monitoring; disabling service.");
                return;
            }
            
            var pattern = "you are not authorized to \\(de\\)op \x02" +
                             this.ircClient.Nickname.ToLower() + "\x02 on \x02(?<channel>#[^\\t ]+)\x02";

            this.noChanopsPattern = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            this.ircClient.ModeReceivedEvent += this.OnModeReceived;
            this.ircClient.JoinReceivedEvent += this.OnJoinReceived;
            this.ircClient.PartReceivedEvent += this.OnPartReceived;
            this.ircClient.ReceivedIrcMessage += this.OnMessageReceived;
            this.ircClient.ReceivedMessage += this.OnNoticeReceived;
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

        private void OnNoticeReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!e.IsNotice)
            {
                return;
            }
            
            this.logger.DebugFormat("Received notice from {0} for {1} with content: {2}", e.User, e.Target, e.Message);

            if (e.User.Nickname == "ChanServ")
            {
                var match = this.noChanopsPattern.Match(e.Message);
                if (match.Success)
                {
                    // oops, we don't have ops here.
                    string channel = match.Groups["channel"].Value;
                    this.logger.ErrorFormat("Cannot obtain ops for channel {0}, skipping", channel);

                    this.ircClient.SendMessage(
                        this.watchedChannels[channel.ToLowerInvariant()],
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

        private void OnMessageReceived(object sender, IrcMessageReceivedEventArgs e)
        {
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

        #region Op requests

        public void RequestPersistentOps(string channel, IChannelOperator requester, string token)
        {
            this.RequestPersistentOps(channel, requester, token, false);
        }
        
        public void RequestPersistentOps(string channel, IChannelOperator requester, string token, bool priority)
        {
            bool existingOps = false;
            
            lock (this.channelStatus)
            {
                if (!this.channelStatus.ContainsKey(channel))
                {
                    return;
                }

                this.channelStatus[channel].PersistentChanops.Add(token, requester);

                existingOps = this.channelStatus[channel].BotOpsHeld;
            }

            if (existingOps)
            {
                // We're already opped, so fire this event immediately
                requester.OnChannelOperatorGranted(this, new OppedEventArgs(channel, token, this, this.ircClient));
            }
            
            this.CheckRestrictivity(channel, priority);
        }

        public void ReleasePersistentOps(string channel, string token)
        {
            lock (this.channelStatus)
            {
                if (!this.channelStatus.ContainsKey(channel))
                {
                    return;
                }

                this.channelStatus[channel].PersistentChanops.Remove(token);
            }
            
            this.CheckRestrictivity(channel);
        }

        public void PerformAsOperator(string channel, Action<IIrcClient> tasks, bool priority)
        {
            this.RequestPersistentOps(channel, new ChanOpTaskList(tasks), Guid.NewGuid().ToString(), priority);
        }
        
        public void PerformAsOperator(string channel, Action<IIrcClient> tasks)
        {
            this.PerformAsOperator(channel, tasks, false);
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

            Dictionary<string, IChannelOperator> channelOperatorTasks;
            lock (this.channelStatus)
            {
                channelOperatorTasks =
                    new Dictionary<string, IChannelOperator>(this.channelStatus[channel].PersistentChanops);
            }

            foreach (var task in channelOperatorTasks)
            {
                task.Value.OnChannelOperatorGranted(this, new OppedEventArgs(channel, task.Key, this, this.ircClient));
            }
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

        private void CheckRestrictivity(string channel, bool priority = false)
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

                if (status.Bans.Contains("$~a") || status.Quiets.Contains("$~a") || status.Quiets.Contains("*!*@*"))
                {
                    needsOps = true;
                }

                if (status.PersistentChanops.Count > 0)
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
                this.ircClient.SendMessage("ChanServ", "op " + channel, null, priority);
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

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}