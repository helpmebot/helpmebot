// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIrcClient.cs" company="Helpmebot Development Team">
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
//   The IrcClient interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Messages;
    using Helpmebot.IRC.Model;

    /// <summary>
    /// The IRC Client interface.
    /// </summary>
    public interface IIrcClient
    {
        /// <summary>
        /// The received message.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> ReceivedMessage;

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        string Nickname { get; set; }

        /// <summary>
        /// Gets the channels.
        /// </summary>
        Dictionary<string, Channel> Channels { get; }

        /// <summary>
        /// Gets a value indicating whether the nick tracking is valid.
        /// </summary>
        bool NickTrackingValid { get; }

        /// <summary>
        /// Gets the user cache.
        /// </summary>
        Dictionary<string, IrcUser> UserCache { get; }

        /// <summary>
        /// Gets a value indicating whether the client logged in to a nickserv account
        /// </summary>
        bool ServicesLoggedIn { get; }

        /// <summary>
        /// The join.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        void JoinChannel(string channel);

        /// <summary>
        /// The part channel.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        void PartChannel(string channel, string message);

        /// <summary>
        /// The send message.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        void SendMessage(string destination, string message);

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Send(IMessage message);
    }
}