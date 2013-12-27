// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Notify.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Defines the Notify type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Collections.Generic;

    using Helpmebot;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The notify.
    /// </summary>
    internal class Notify : GenericCommand
    {
        /// <summary>
        /// The requested notifications.
        /// </summary>
        private static readonly Dictionary<string, List<LegacyUser>> RequestedNotifications = new Dictionary<string, List<LegacyUser>>();

        /// <summary>
        /// The lock for the requested notifications dictionary.
        /// </summary>
        private static readonly object NotificationsDictionaryLock = new object();

        /// <summary>
        /// Initialises a new instance of the <see cref="Notify"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Notify(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// The notify join.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        internal void NotifyJoin(LegacyUser source, string channel)
        {
            List<LegacyUser> toNotify;
            lock (NotificationsDictionaryLock)
            {
                if (RequestedNotifications.TryGetValue(source.Nickname.ToUpperInvariant(), out toNotify))
                {
                    RequestedNotifications.Remove(source.Nickname);
                }
            }

            if (toNotify != null)
            {
                string message = this.MessageService.RetrieveMessage("notifyJoin", this.Channel, new[] { source.Nickname, channel });
                foreach (LegacyUser user in toNotify)
                {
                    Helpmebot6.irc.IrcPrivmsg(user.Nickname, message);
                }
            }
        }

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length != 1)
            {
                return new CommandResponseHandler(this.MessageService.RetrieveMessage("argsExpected1", this.Channel, new[] { "nickname" }));
            }

            string trigger;
            lock (NotificationsDictionaryLock)
            {
                LegacyUser toNotify = this.Source;
                trigger = this.Arguments[0];
                string triggerUpper = trigger.ToUpperInvariant();
                if (!RequestedNotifications.ContainsKey(trigger))
                {
                    RequestedNotifications.Add(triggerUpper, new List<LegacyUser>());
                }

                RequestedNotifications[triggerUpper].Add(toNotify);
            }

            return new CommandResponseHandler(this.MessageService.RetrieveMessage("confirmNotify", this.Channel, new[] { trigger }));
        }
    }
}
