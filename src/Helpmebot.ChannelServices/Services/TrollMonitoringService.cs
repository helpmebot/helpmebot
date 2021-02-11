namespace Helpmebot.ChannelServices.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.ExtensionMethods;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class TrollMonitorService : ITrollMonitoringService
    {
        private readonly IIrcClient client;
        private readonly ILogger logger;
        private List<IPNetwork> networks;

        private Dictionary<IUser, uint> trackedUsers = new Dictionary<IUser, uint>();
        private Regex emojiRegex;
        private Regex badWordRegex;

        private string targetChannel = "#wikipedia-en-help";

        public TrollMonitorService(IIrcClient client, ILogger logger)
        {
            this.client = client;
            this.logger = logger;

            this.networks = new List<IPNetwork>
            {
                IPNetwork.Parse("103.139.56.0/24"),
                IPNetwork.Parse("110.235.229.0/24"),
                IPNetwork.Parse("110.235.230.0/24"),
                IPNetwork.Parse("47.8.32.0/19"),
                IPNetwork.Parse("47.9.128.0/19"),
                IPNetwork.Parse("47.15.0.0/16"),
                IPNetwork.Parse("49.37.128.0/19")
            };

            this.emojiRegex = new Regex("(\\u00a9|\\u00ae|[\\u2000-\\u3300]|\\ud83c[\\ud000-\\udfff]|\\ud83d[\\ud000-\\udfff]|\\ud83e[\\ud000-\\udfff])", RegexOptions.IgnoreCase);
            
            this.badWordRegex = new Regex("(?: |^)(cock|pussy|fuck|hard core|hardcore|babes|dick|ur mom)(?: |$)", RegexOptions.IgnoreCase);
        }

        private void ClientOnReceivedMessage(object sender, MessageReceivedEventArgs e)
        {
            if (e.Target != this.targetChannel)
            {
                // don't care about other channels.
                return;
            }

            if (!this.trackedUsers.ContainsKey(e.User))
            {
                // don't care about non-tracked users
                return;
            }

            var emojiMatch = this.emojiRegex.Match(e.Message);
            if (emojiMatch.Success)
            {
                this.client.SendNotice("stwalkerster", $"Tracked user {e.User} in -en-help SENT EMOJI");
                this.trackedUsers[e.User]++;
            }

            var badWordMatch = this.badWordRegex.Match(e.Message);
            if (badWordMatch.Success)
            {
                this.client.SendNotice("stwalkerster", $"Tracked user {e.User} in -en-help SENT BADWORD");
                this.trackedUsers[e.User]++;
            }

            if (this.trackedUsers[e.User] >= 5) 
            {
                this.client.SendNotice("stwalkerster", $"Tracked user {e.User} in -en-help   *** PROPOSED BAN ***");
            }
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
            
                this.client.SendNotice("stwalkerster", $"Tracked user {user} in -en-help from target ranges");
                return;
            }
            
            if (user.Nickname.Contains("AmberRhino"))
            {
                this.logger.DebugFormat("Tracking {0}, nickname matches AmberRhino*", user);
                this.trackedUsers.Add(user, 0);
            
                this.client.SendNotice("stwalkerster", $"Tracked user {user} in -en-help by nickname pattern");
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