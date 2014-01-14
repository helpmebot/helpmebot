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
        /// The data received event.
        /// </summary>
        event EventHandler<DataReceivedEventArgs> DataReceivedEvent;

        /// <summary>
        /// The unrecognised data received event.
        /// </summary>
        event EventHandler<DataReceivedEventArgs> UnrecognisedDataReceivedEvent;

        /// <summary>
        /// The connection registration succeeded event.
        /// </summary>
        event EventHandler ConnectionRegistrationSucceededEvent;

        /// <summary>
        /// The ping event.
        /// </summary>
        event IrcAccessLayer.PingEventHandler PingEvent;

        /// <summary>
        /// The nickname change event.
        /// </summary>
        event IrcAccessLayer.NicknameChangeEventHandler NicknameChangeEvent;

        /// <summary>
        /// The mode change event.
        /// </summary>
        event IrcAccessLayer.ModeChangeEventHandler ModeChangeEvent;

        /// <summary>
        /// The quit event.
        /// </summary>
        event IrcAccessLayer.QuitEventHandler QuitEvent;

        /// <summary>
        /// The join event.
        /// </summary>
        event IrcAccessLayer.JoinEventHandler JoinEvent;

        /// <summary>
        /// The part event.
        /// </summary>
        event IrcAccessLayer.PartEventHandler PartEvent;

        /// <summary>
        /// The topic event.
        /// </summary>
        event IrcAccessLayer.TopicEventHandler TopicEvent;

        /// <summary>
        /// The invite event.
        /// </summary>
        event IrcAccessLayer.InviteEventHandler InviteEvent;

        /// <summary>
        /// The kick event.
        /// </summary>
        event IrcAccessLayer.KickEventHandler KickEvent;

        /// <summary>
        /// The private message event.
        /// </summary>
        event EventHandler<PrivateMessageEventArgs> PrivateMessageEvent;

        /// <summary>
        /// The client to client event.
        /// </summary>
        event EventHandler<PrivateMessageEventArgs> ClientToClientEvent;

        /// <summary>
        /// The notice event.
        /// </summary>
        event EventHandler<PrivateMessageEventArgs> NoticeEvent;

        /// <summary>
        /// The err nickname in use event.
        /// </summary>
        event EventHandler ErrNicknameInUseEvent;

        /// <summary>
        /// The err unavailable resource.
        /// </summary>
        event EventHandler ErrUnavailResource;

        /// <summary>
        /// The thread fatal error event.
        /// </summary>
        event EventHandler ThreadFatalErrorEvent;

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        string Nickname { get; set; }

        /// <summary>
        /// Gets the message count.
        /// </summary>
        int MessageCount { get; }
        
        /// <summary>
        /// The connect.
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

        /// <summary>
        /// Checks if nickname is on channel
        /// </summary>
        /// <param name = "channel">channel to check</param>
        /// <param name = "nickname">nickname to check</param>
        /// <returns>1 if nickname is on channel
        /// 0 if nickname is not on channel
        /// -1 if it cannot be checked at the moment
        /// </returns>
        int IsOnChannel(string channel, string nickname);
    }
}