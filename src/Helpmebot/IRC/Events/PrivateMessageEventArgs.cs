// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivateMessageEventArgs.cs" company="Helpmebot Development Team">
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
//   Defines the PrivateMessageEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Events
{
    using System;

    using Helpmebot.Legacy.Model;

    /// <summary>
    /// The private message event args.
    /// </summary>
    public class PrivateMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="PrivateMessageEventArgs"/> class.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public PrivateMessageEventArgs(LegacyUser sender, string destination, string message)
        {
            this.Message = message;
            this.Destination = destination;
            this.Sender = sender;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the destination.
        /// </summary>
        public string Destination { get; private set; }

        /// <summary>
        /// Gets the sender.
        /// </summary>
        public LegacyUser Sender { get; private set; }
    }
}
