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
            { "##stwalkerster", "##stwalkerster-development" }
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
            this.ircClient.ReceivedMessage += this.OnReceivedMessage;
            this.logger.Info("Hooked into IRC events");
        }

        public void ResyncChannel(string channel)
        {
            this.logger.InfoFormat("Resyncing channel status for {0}", channel);

            lock (this.channelStatus)
            {
                this.ircClient.Mode(channel, "-o " + this.ircClient.Nickname);

                this.logger.InfoFormat("Requesting chanops for {0}", channel);
                this.ircClient.SendMessage("ChanServ", string.Format("OP {0} {1}", channel, this.ircClient.Nickname));
                this.channelStatus[channel].BotOpsRequested = true;
                this.channelStatus[channel].FirstTimeSyncComplete = false;

                this.logger.InfoFormat("Requesting ban/quiet lists for {0}", channel);
                this.ircClient.Mode(channel, "");
                this.ircClient.Mode(channel, "b");
                this.ircClient.Mode(channel, "q");
            }
        }

        private void OnPartReceived(object sender, JoinEventArgs e)
        {
            if (e.User.Nickname != this.ircClient.Nickname)
            {
                return;
            }

            this.logger.InfoFormat("Left channel {0}, ending tracking", e.Channel);

            lock (this.channelStatus)
            {
                this.channelStatus.Remove(e.Channel.ToLowerInvariant());
            }
        }

        private void OnReceivedMessage(object sender, MessageReceivedEventArgs e)
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

                readyToAnalyse = this.channelStatus[channel].ExemptListDownloaded
                                 && this.channelStatus[channel].BanListDownloaded
                                 && this.channelStatus[channel].QuietListDownloaded
                                 && (!this.channelStatus[channel].FirstTimeSyncComplete);
            }

            if (readyToAnalyse)
            {
                this.PerformFirstTimeAnalysis(channel, false);
            }
        }

        private void FixChannel(ChannelStatus status, string channel, string modechange, bool changeSettings, string message)
        {
            if (changeSettings)
            {
                status.LastOverrideTime = DateTime.Now;
                this.ircClient.Mode(channel, modechange);
            }
            else
            {
                this.ircClient.SendMessage(this.watchedChannels[channel], string.Format(message, channel));
            }
        }

        private void PerformFirstTimeAnalysis(string channel, bool changeSettings)
        {
            this.logger.InfoFormat("Performing analysis for channel {0}", channel);
            
            lock (this.channelStatus)
            {
                var status = this.channelStatus[channel];
                status.FirstTimeSyncComplete = true;

             //   if (status.Moderated && !status.ReducedModeration)
             //   {
             //       string message =
             //               "WARNING - the channel is currently configured as +m, but without +z. This means that helpees will be unable to speak. Ops: please consider +q $~a with an exemption for gateway users instead.";
             //       this.FixChannel(status, channel, "-m+qez $~a *!*@gateway/web/*", changeSettings, message);
             //       return;
             //   }
             //   
             //   if (status.RegisteredOnly)
             //   {
             //       this.FixChannel(status, channel, "-r+qez $~a *!*@gateway/web/*", changeSettings, "WARNING - the channel is currently configured as +r. This means that only NickServ-authed users will be able to join, but not helpees. Ops: please consider +q $~a with an exemption for gateway users instead.");
             //   }
             //
             //   var channelOpCount =
             //       this.ircClient.Channels[channel].Users.Count(
             //           x => x.Value.Operator && x.Key != this.ircClient.Nickname && x.Key != "ChanServ");
             //   
             //   if (status.Moderated && status.ReducedModeration && channelOpCount < 1)
             //   {
             //       // WARN channel closed, no ops and +mz
             //       this.FixChannel(status, channel, "-m+qe $~a *!*@gateway/web/*", changeSettings, "WARNING - the channel is currently configured as +mz, but nobody is opped. Helpees will be requesing help, but nobody will hear them. Ops: please consider +q $~a with an exemption for gateway users instead.");
             //       return;
             //   }
             //
             //   if (status.Moderated && status.ReducedModeration && channelOpCount > 0)
             //   {
             //       // WARN +mz, consider +q $~a with exemption to allow others to help.
             //       this.FixChannel(status, channel, "-m+qe $~a *!*@gateway/web/*", changeSettings, "WARNING - the channel is currently configured as +mz. Helpees will be requesing help, but only opped users will hear them. Ops: please consider +q $~a with an exemption for gateway users instead.");
             //       return;
             //   }
             //   
             //   if (status.Quiets.Contains("$~a"))
             //   {
             //       if (status.Exempts.Exists(x => x.Contains("kiwiirc.com") || x.Contains("gateway/")))
             //       {
             //           // quiet with exemption, OK.
             //       }
             //       else
             //       {
             //           if (channelOpCount == 0)
             //           {
             //               if (status.ReducedModeration)
             //               {
             //                   this.FixChannel(status, channel, "+e *!*@gateway/web/*", changeSettings, "WARNING - the channel is currently configured as +qz $~a, but without an exemption. Helpees will be requesing help, but nobody is opped to hear them. Ops: please consider an exemption for gateway users too.");
             //                   return;
             //               }
             //               else
             //               {
             //                   this.FixChannel(status, channel, "+ze *!*@gateway/web/*", changeSettings, "WARNING - the channel is currently configured as +q $~a. Helpees will be unable to speak. Ops: please consider an exemption for gateway users too.");
             //                   return;
             //               }
             //           }
             //           else
             //           {
             //               // WARN quiet with no exemption
             //               if (status.ReducedModeration)
             //               {
             //                   this.FixChannel(status, channel, "+e *!*@gateway/web/*", changeSettings, "WARNING - the channel is currently configured as +q $~a, but without an exemption. Helpees will be requesing help, but they will only be heard by ops. Ops: please consider an exemption for gateway users too.");
             //                   return;
             //               }
             //               else
             //               {
             //                   this.FixChannel(status, channel, "+ze *!*@gateway/web/*", changeSettings, "WARNING - the channel is currently configured as +q $~a, but without an exemption. Helpees will unable to speak. Ops: please consider an exemption for gateway users too.");
             //                   return;
             //               }
             //           }
             //       }
             //   }
             //
             //   if (status.Bans.Contains("$~a"))
             //   {
             //       if (status.Exempts.Exists(x => x.Contains("kiwiirc.com") || x.Contains("gateway/")))
             //       {
             //           // ban with exemption, OK
             //       }
             //       else
             //       {
             //           // WARN channel closed
             //           this.FixChannel(status, channel, "+ze *!*@gateway/web/*", changeSettings, "WARNING - the channel is currently configured as +b $~a, but without an exemption. Helpees will be unable to join the channel. Ops: please consider +q $~a with an exemption for gateway users instead.");
             //           return;
             //       }
             //   }
            }
        }

        private void PerformChangeAnalysis(string channel, ModeChanges changes)
        {
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

                    this.logger.InfoFormat("Requesting chanops for {0}", channel);
                    this.ircClient.SendMessage(
                        "ChanServ",
                        string.Format("OP {0} {1}", channel, this.ircClient.Nickname));
                    this.channelStatus[channel].BotOpsRequested = true;

                    this.logger.InfoFormat("Requesting ban/quiet lists for {0}", channel);
                    this.ircClient.Mode(channel, "b");
                    this.ircClient.Mode(channel, "q");
                }

            }
        }

        private void OnModeReceived(object sender, ModeEventArgs modeEventArgs)
        {
            var channel = modeEventArgs.Target.ToLowerInvariant();

            if (this.watchedChannels.ContainsKey(channel))
            {
                var changes = ModeChanges.FromChangeList(modeEventArgs.Changes);
                
                lock (this.channelStatus)
                {
                    if (this.channelStatus[channel].BotOpsRequested && changes.Ops.Contains(this.ircClient.Nickname))
                    {
                        this.logger.DebugFormat("Opped in {0}, requesting exemption list.", channel);
                        this.ircClient.Mode(channel, "e");
                    }

                    this.SyncChangesToChannel(this.channelStatus[channel], changes);
                }
                
                this.logger.DebugFormat("Mode change seen on {0}: {1}",
                    channel,
                    string.Join(" ", modeEventArgs.Changes));

                this.PerformChangeAnalysis(channel, changes);
            }
        }

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