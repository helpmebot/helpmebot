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

    /// <summary>
    /// The notify.
    /// </summary>
    internal class Notify : GenericCommand
    {
        public Notify(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// The requested notifications.
        /// </summary>
        private static readonly Dictionary<string, List<User>> RequestedNotifications = new Dictionary<string, List<User>>();

        /// <summary>
        /// The lock for the requested notifications dictionary.
        /// </summary>
        private static readonly object NotificationsDictionaryLock = new object();

        /// <summary>
        /// The notify join.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        internal void NotifyJoin(User source, string channel)
        {
            List<User> toNotify;
            lock (NotificationsDictionaryLock)
            {
                if (RequestedNotifications.TryGetValue(source.nickname.ToUpperInvariant(), out toNotify))
                {
                    RequestedNotifications.Remove(source.nickname);
                }
            }

            if (toNotify != null)
            {
                Message msgprovider = new Message();
                string message = msgprovider.get("notifyJoin", new[] { source.nickname, channel });
                foreach (User user in toNotify)
                {
                    Helpmebot6.irc.ircPrivmsg(user.nickname, message);
                }
            }
        }

        /// <summary>
        /// The execute command.
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
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            Message msgprovider = new Message();
            if (args.Length != 1)
            {
                return new CommandResponseHandler(msgprovider.get("argsExpected1", new[] { "nickname" }));
            }

            string trigger;
            lock (NotificationsDictionaryLock)
            {
                User toNotify = source;
                trigger = args[0];
                string triggerUpper = trigger.ToUpperInvariant();
                if (!RequestedNotifications.ContainsKey(trigger))
                {
                    RequestedNotifications.Add(triggerUpper, new List<User>());
                }

                RequestedNotifications[triggerUpper].Add(toNotify);
            }

            return new CommandResponseHandler(msgprovider.get("confirmNotify", new[] { trigger }));
        }
    }
}
