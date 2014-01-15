// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JoinEventArgs.cs" company="Helpmebot Development Team">
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
//   Defines the JoinEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Events
{
    using Helpmebot.IRC.Messages;
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The join event args.
    /// </summary>
    public class JoinEventArgs : UserEventArgsBase
    {
        /// <summary>
        /// The channel.
        /// </summary>
        private readonly string channel;

        /// <summary>
        /// Initialises a new instance of the <see cref="JoinEventArgs"/> class.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="user">
        ///     The user.
        /// </param>
        /// <param name="channel">
        ///     The channel.
        /// </param>
        public JoinEventArgs(IMessage message, IUser user, string channel)
            : base(message, user)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Gets the channel.
        /// </summary>
        public string Channel
        {
            get
            {
                return this.channel;
            }
        }
    }
}
