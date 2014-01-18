// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIrcAccessLayer.cs" company="Helpmebot Development Team">
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
//   Defines the IIrcAccessLayer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Legacy.IRC
{
    using System;

    using Helpmebot.IRC.Events;

    /// <summary>
    /// The IRC Access Layer interface.
    /// </summary>
    public interface IIrcAccessLayer
    {
        /// <summary>
        /// The connection registration succeeded event.
        /// </summary>
        event EventHandler ConnectionRegistrationSucceededEvent;

        /// <summary>
        /// The join event.
        /// </summary>
        event EventHandler<JoinEventArgs> JoinEvent;
        
        /// <summary>
        /// The invite event.
        /// </summary>
        event EventHandler<InviteEventArgs> InviteEvent;
        
        /// <summary>
        /// The private message event.
        /// </summary>
        event EventHandler<PrivateMessageEventArgs> PrivateMessageEvent;

        /// <summary>
        /// The notice event.
        /// </summary>
        event EventHandler<PrivateMessageEventArgs> NoticeEvent;

        /// <summary>
        /// The thread fatal error event.
        /// </summary>
        event EventHandler ThreadFatalErrorEvent;

        /// <summary>
        /// Gets the nickname.
        /// </summary>
        string Nickname { get; }

        /// <summary>
        /// The Connect.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool Connect();

        /// <summary>
        /// The send raw line.
        /// </summary>
        /// <param name="line">
        /// The line.
        /// </param>
        void SendRawLine(string line);

        /// <summary>
        ///   Sends a private message
        /// </summary>
        /// <param name = "destination">The destination of the private message.</param>
        /// <param name = "message">The message text to be sent</param>
        void IrcPrivmsg(string destination, string message);
        
        /// <summary>
        /// The IRC join.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        void IrcJoin(string channel);

        /// <summary>
        /// The IRC part.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        void IrcPart(string channel, string message);
        
        /// <summary>
        /// The IRC notice.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        void IrcNotice(string destination, string message);
    }
}