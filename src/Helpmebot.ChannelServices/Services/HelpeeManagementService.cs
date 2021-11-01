namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Configuration;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.Configuration;
    using Prometheus;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model;

    public class HelpeeManagementService : IHelpeeManagementService
    {
        private static readonly Gauge HelpeeCount = Metrics.CreateGauge(
            "helpmebot_helpeemanagement_helpees",
            "The number of helpees in the channel",
            new GaugeConfiguration {SuppressInitialValue = true});

        private static readonly Gauge HelperCount = Metrics.CreateGauge(
            "helpmebot_helpeemanagement_helpers",
            "The number of helpers in the channel",
            new GaugeConfiguration {SuppressInitialValue = true});

        private static readonly Gauge IgnoredCount = Metrics.CreateGauge(
            "helpmebot_helpeemanagement_ignored",
            "The number of ignored users in the channel",
            new GaugeConfiguration {SuppressInitialValue = true});

        private static readonly Gauge UserMismatchCount = Metrics.CreateGauge(
            "helpmebot_helpeemanagement_usercount_mismatch",
            "The size of any mismatch between IRC client channel users, and tracked users.",
            new GaugeConfiguration {SuppressInitialValue = true});
        
        private readonly IIrcClient client;
        private readonly ILogger logger;

        private readonly Dictionary<IrcUser, DateTime> helpeeIdleCache = new Dictionary<IrcUser, DateTime>();
        private readonly Dictionary<IrcUser, DateTime> helperIdleCache = new Dictionary<IrcUser, DateTime>();
        private readonly List<IrcUser> ignoredInChannel = new List<IrcUser>();

        private readonly List<string> ignoreList;
        private readonly string monitoringChannel;
        private readonly string targetChannel;
        
        private readonly object lockToken = new object();
        

        public HelpeeManagementService(IIrcClient client, ILogger logger, HelpeeManagementConfiguration configuration)
        {
            this.client = client;
            this.logger = logger;
            
            this.targetChannel = configuration.TargetChannel;
            this.monitoringChannel = configuration.MonitorChannel;
            this.ignoreList = configuration.IgnoredNicknames;
        }

        public IDictionary<IrcUser, DateTime> Helpees
        {
            get
            {
                ReadOnlyDictionary<IrcUser, DateTime> idleCache;
                lock (this.lockToken)
                {
                    idleCache = new ReadOnlyDictionary<IrcUser, DateTime>(this.helpeeIdleCache);
                }

                return idleCache;
            }
        }
        
        public IDictionary<IrcUser, DateTime> Helpers
        {
            get
            {
                ReadOnlyDictionary<IrcUser, DateTime> idleCache;
                lock (this.lockToken)
                {
                    idleCache = new ReadOnlyDictionary<IrcUser, DateTime>(this.helperIdleCache);
                }

                return idleCache;
            }
        }

        #region IRC tracking
        private void IrcJoinReceived(object sender, JoinEventArgs e)
        {
            if (e.Channel == this.targetChannel && e.User.Nickname == e.Client.Nickname)
            {
                lock (this.lockToken)
                {
                    // we're joining.
                    this.helpeeIdleCache.Clear();
                    this.helperIdleCache.Clear();
                    this.ignoredInChannel.Clear();

                    this.SyncCounts();
                }

                return;
            }
            
            if (e.Channel != this.targetChannel)
            {
                // not for this channel
                return;
            }
         
            this.logger.InfoFormat("Seen join of {0} to {1}", e.User, e.Channel);
            lock (this.lockToken)
            {
                if (this.ignoreList.Contains(e.User.Nickname) || e.User.Nickname == this.client.Nickname)
                {
                    this.logger.DebugFormat("{0} is ignorable.", e.User);
                    this.ignoredInChannel.Add((IrcUser) e.User);
                }
                else
                {
                    this.helpeeIdleCache.Add((IrcUser) e.User, DateTime.UtcNow);
                }
                
                this.SyncCounts();
            }
        }

        private void IrcMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Target != this.targetChannel)
            {
                // not for us
                return;
            }

            var ircUser = (IrcUser) e.User;
            lock (this.lockToken)
            {
                if (this.helpeeIdleCache.ContainsKey(ircUser))
                {
                    this.helpeeIdleCache[ircUser] = DateTime.UtcNow;
                }

                if (this.helperIdleCache.ContainsKey(ircUser))
                {
                    this.helperIdleCache[ircUser] = DateTime.UtcNow;
                }
            }
        }
        
        private void IrcWasKicked(object sender, KickedEventArgs e)
        {
            if (e.Channel != this.targetChannel)
            {
                // not for us
                return;
            }
            
            // oops.
            this.logger.Warn("We've been kicked from the channel!?");
            this.StopTracking();
        }
        
        private void ChannelUserModeEvent(object sender, ChannelUserModeEventArgs e)
        {
            if (e.Channel != this.targetChannel)
            {
                this.logger.Debug("Channel mode not target channel");
                return;
            }

            if (e.ModeFlag == "v")
            {
                var affectedUser = (IrcUser) e.AffectedUser;

                if (this.ignoreList.Contains(affectedUser.Nickname) || affectedUser.Nickname == this.client.Nickname)
                {
                    this.logger.Debug("Channel mode ignored");
                    return;
                }
                
                lock (this.lockToken)
                {
                    if (e.Adding)
                    {
                        var idleDate = this.helpeeIdleCache[affectedUser];
                        this.helpeeIdleCache.Remove(affectedUser);
                        this.helperIdleCache.Add(affectedUser, idleDate);
                    }
                    else
                    {
                        var idleDate = this.helperIdleCache[affectedUser];
                        this.helperIdleCache.Remove(affectedUser);
                        this.helpeeIdleCache.Add(affectedUser, idleDate);
                    }
                    
                    this.SyncCounts();
                }
            }
            else
            {
                this.logger.Debug("Channel mode not voice");
            }
        }

        private void IrcQuitReceived(object sender, QuitEventArgs e)
        {
            this.logger.Info($"Seen quit of {e.User}. Quits are all channels, so removing from tracking.");
            this.LostUser((IrcUser)e.User);
        }

        private void IrcKickReceived(object sender, KickEventArgs e)
        {
            if (e.Channel != this.targetChannel)
            {
                // not for us
                return;
            }

            this.logger.DebugFormat("Seen kick for {0}", e.KickedUser);
            this.LostUser((IrcUser)e.KickedUser);
        }

        private void IrcPartReceived(object sender, JoinEventArgs e)
        {
            if (e.Channel != this.targetChannel)
            {
                // not for us
                return;
            }

            if (e.User.Nickname == e.Client.Nickname)
            {
                // oops.
                this.logger.Warn("We've parted from the channel!?");
                this.StopTracking();
                return;
            }

            this.logger.DebugFormat("Removing user...");
            var ircUser = (IrcUser) e.User;
            this.LostUser(ircUser);
        }

        private void LostUser(IrcUser ircUser)
        {
            lock (this.lockToken)
            {
                bool done = false;
                if (this.helpeeIdleCache.ContainsKey(ircUser))
                {
                    this.helpeeIdleCache.Remove(ircUser);
                    this.logger.DebugFormat("Removed {0} from helpee cache", ircUser);
                    done = true;
                }

                if (this.helperIdleCache.ContainsKey(ircUser))
                {
                    this.helperIdleCache.Remove(ircUser);
                    this.logger.DebugFormat("Removed {0} from helper cache", ircUser);
                    done = true;
                }

                if (this.ignoredInChannel.Contains(ircUser))
                {
                    this.ignoredInChannel.Remove(ircUser);
                    this.logger.DebugFormat("Removed {0} from ignored cache", ircUser);
                    done = true;
                }

                if (!done)
                {
                    var helpeeList = this.helpeeIdleCache.Count == 0
                        ? "(none)"
                        : this.helpeeIdleCache.Aggregate("", (cur, next) => cur + ", " + next.Key).Substring(2);
                    var helperList = this.helperIdleCache.Count == 0
                        ? "(none)"
                        : this.helperIdleCache.Aggregate("", (cur, next) => cur + ", " + next.Key).Substring(2);
                    var ignoredList = this.ignoredInChannel.Count == 0
                        ? "(none)"
                        : this.ignoredInChannel.Aggregate("", (cur, next) => cur + ", " + next).Substring(2);

                    int? resultHash = null;
                    foreach (var pairs in this.helperIdleCache)
                    {
                        if (pairs.Key.Nickname == ircUser.Nickname)
                        {
                            resultHash = pairs.Key.GetHashCode();
                        }
                    }
                    foreach (var pairs in this.helpeeIdleCache)
                    {
                        if (pairs.Key.Nickname == ircUser.Nickname)
                        {
                            resultHash = pairs.Key.GetHashCode();
                        }
                    }

                    this.logger.DebugFormat(
                        "Removal of {0} (hash {4}) from tracking was requested, but not executed. Lost hash caches: {5}. Helpees: {1} | Helpers: {2} | Ignored: {3}",
                        ircUser,
                        helpeeList,
                        helperList,
                        ignoredList,
                        ircUser.GetHashCode(),
                        resultHash.HasValue ? resultHash.Value.ToString() : "null");
                }
                 
                this.SyncCounts();
            }
        }

        private void EndOfWhoReceived(object sender, EndOfWhoEventArgs e)
        {
            if (e.Channel != this.targetChannel)
            {
                return;
            }

            lock (this.lockToken)
            {
                foreach (var entry in e.Client.Channels[this.targetChannel].Users)
                {
                    if (this.ignoreList.Contains(entry.Value.User.Nickname) || entry.Value.User.Nickname == this.client.Nickname)
                    {
                        this.ignoredInChannel.Add(entry.Value.User);
                        continue;
                    }

                    if (entry.Value.Voice)
                    {
                        this.helperIdleCache.Add(entry.Value.User, DateTime.MinValue);
                    }
                    else
                    {
                        this.helpeeIdleCache.Add(entry.Value.User, DateTime.MinValue);
                    }
                }
                
                this.SyncCounts();
            }
        }

        private void SyncCounts()
        {
            IgnoredCount.Set(this.ignoredInChannel.Count);
            HelperCount.Set(this.helperIdleCache.Count);
            HelpeeCount.Set(this.helpeeIdleCache.Count);

            if (this.client.Channels.ContainsKey(this.targetChannel))
            {
                var usersCount = this.client.Channels[this.targetChannel].Users.Count;
                UserMismatchCount.Set(
                    this.ignoredInChannel.Count + this.helperIdleCache.Count + this.helpeeIdleCache.Count
                    - usersCount);
            }
            else
            {
                UserMismatchCount.Unpublish();
            }

            var helpeeList = this.helpeeIdleCache.Count == 0
                ? ""
                : this.helpeeIdleCache.Aggregate("", (cur, next) => cur + ", " + next.Key.Nickname).Substring(2);
            var helperList = this.helperIdleCache.Count == 0
                ? ""
                : this.helperIdleCache.Aggregate("", (cur, next) => cur + ", " + next.Key.Nickname).Substring(2);
            var ignoredList = this.ignoredInChannel.Count == 0
                ? ""
                : this.ignoredInChannel.Aggregate("", (cur, next) => cur + ", " + next.Nickname).Substring(2);
            
            
            this.logger.TraceFormat("Helpee cache: {0}", helpeeList);
            this.logger.TraceFormat("Helper cache: {0}", helperList);
            this.logger.TraceFormat("Ignore cache: {0}", ignoredList);
        }

        void StopTracking()
        {
            this.logger.Warn("Helpee tracking suspended.");
            lock (this.lockToken)
            {
                this.helpeeIdleCache.Clear();
                this.helperIdleCache.Clear();
                this.ignoredInChannel.Clear();

                this.SyncCounts();
                
                HelpeeCount.Unpublish();
                HelperCount.Unpublish();
                IgnoredCount.Unpublish();
            }
        }
        #endregion
        #region Startable service
        public void Start()
        {
            if (this.targetChannel == null)
            {
                this.logger.Warn("Helpee management service not configured; not starting");
                return;
            }
            
            this.logger.Debug("Starting helpee management service");
            this.client.JoinReceivedEvent += this.IrcJoinReceived;
            this.client.EndOfWhoEvent += this.EndOfWhoReceived;
            this.client.PartReceivedEvent += this.IrcPartReceived;
            this.client.KickReceivedEvent += this.IrcKickReceived;
            this.client.QuitReceivedEvent += this.IrcQuitReceived;
            this.client.WasKickedEvent += this.IrcWasKicked;
            this.client.ChannelUserModeEvent += this.ChannelUserModeEvent;
            this.client.ReceivedMessage += this.IrcMessageReceived;
        }

        public void Stop()
        {           
            this.logger.Debug("Stopping helpee management service");
            this.client.JoinReceivedEvent -= this.IrcJoinReceived;
            this.client.EndOfWhoEvent += this.EndOfWhoReceived;
            this.client.PartReceivedEvent -= this.IrcPartReceived;
            this.client.KickReceivedEvent -= this.IrcKickReceived;
            this.client.QuitReceivedEvent -= this.IrcQuitReceived;
            this.client.WasKickedEvent -= this.IrcWasKicked;
            this.client.ChannelUserModeEvent += this.ChannelUserModeEvent;
            this.client.ReceivedMessage -= this.IrcMessageReceived;
        }
        #endregion
    }
}