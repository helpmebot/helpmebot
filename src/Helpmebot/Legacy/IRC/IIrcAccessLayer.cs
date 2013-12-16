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
        /// Gets the server info.
        /// </summary>
        string ServerInfo { get; }

        /// <summary>
        /// Gets or sets the client version.
        /// </summary>
        string ClientVersion { get; set; }

        /// <summary>
        /// Gets a value indicating whether is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        string Nickname { get; set; }

        /// <summary>
        /// Gets the username.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the real name.
        /// </summary>
        string RealName { get; }

        /// <summary>
        /// Gets the server.
        /// </summary>
        string Server { get; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        uint Port { get; }

        /// <summary>
        /// Gets the my identity.
        /// </summary>
        string MyIdentity { get; }

        /// <summary>
        /// Gets or sets the flood protection wait time.
        /// </summary>
        int FloodProtectionWaitTime { get; set; }

        /// <summary>
        /// Gets the connection user modes.
        /// </summary>
        /// <remarks>
        /// +4 if receiving wallops, +8 if invisible
        /// </remarks>
        int ConnectionUserModes { get; }

        /// <summary>
        /// Gets the message count.
        /// </summary>
        int MessageCount { get; }

        /// <summary>
        /// Gets the idle time.
        /// </summary>
        TimeSpan IdleTime { get; }

        /// <summary>
        /// Gets a value indicating whether log events.
        /// </summary>
        bool LogEvents { get; }

        /// <summary>
        /// Gets the active channels.
        /// </summary>
        string[] ActiveChannels { get; }

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
        /// The IRC pong.
        /// </summary>
        /// <param name="dataPacket">
        /// The data packet.
        /// </param>
        void IrcPong(string dataPacket);

        /// <summary>
        /// The IRC ping.
        /// </summary>
        /// <param name="dataPacket">
        /// The data packet.
        /// </param>
        void IrcPing(string dataPacket);

        /// <summary>
        ///   Sends a private message
        /// </summary>
        /// <param name = "destination">The destination of the private message.</param>
        /// <param name = "message">The message text to be sent</param>
        void IrcPrivmsg(string destination, string message);

        /// <summary>
        /// The IRC quit.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void IrcQuit(string message);

        /// <summary>
        /// The IRC quit.
        /// </summary>
        void IrcQuit();

        /// <summary>
        /// The IRC join.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        void IrcJoin(string channel);

        /// <summary>
        /// The IRC join.
        /// </summary>
        /// <param name="channels">
        /// The channels.
        /// </param>
        void IrcJoin(string[] channels);

        /// <summary>
        /// The IRC mode.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="modeflags">
        /// The mode flags.
        /// </param>
        /// <param name="param">
        /// The parameter.
        /// </param>
        void IrcMode(string channel, string modeflags, string param);

        /// <summary>
        /// The IRC mode.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        void IrcMode(string channel, string flags);

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
        /// The IRC part.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        void IrcPart(string channel);

        /// <summary>
        /// The part all channels.
        /// </summary>
        void PartAllChannels();

        /// <summary>
        /// The IRC names.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        void IrcNames(string channel);

        /// <summary>
        /// The IRC names.
        /// </summary>
        void IrcNames();

        /// <summary>
        /// The IRC list.
        /// </summary>
        void IrcList();

        /// <summary>
        /// The IRC list.
        /// </summary>
        /// <param name="channels">
        /// The channels.
        /// </param>
        void IrcList(string channels);

        /// <summary>
        /// The IRC invite.
        /// </summary>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        void IrcInvite(string nickname, string channel);

        /// <summary>
        /// The IRC kick.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        void IrcKick(string channel, string user);

        /// <summary>
        /// The IRC kick.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="reason">
        /// The reason.
        /// </param>
        void IrcKick(string channel, string user, string reason);

        /// <summary>
        /// The CTCP reply.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        void CtcpReply(string destination, string command, string parameters);

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
        /// The IRC MOTD.
        /// </summary>
        void IrcMotd();

        /// <summary>
        /// The IRC local users.
        /// </summary>    
        void IrcLusers();

        /// <summary>
        /// The IRC version.
        /// </summary>
        void IrcVersion();

        /// <summary>
        /// The IRC stats.
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        void IrcStats(string query);

        /// <summary>
        /// The IRC links.
        /// </summary>
        /// <param name="mask">
        /// The mask.
        /// </param>
        void IrcLinks(string mask);

        /// <summary>
        /// The IRC time.
        /// </summary>
        void IrcTime();

        /// <summary>
        /// The IRC topic.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        void IrcTopic(string channel);

        /// <summary>
        /// The IRC topic.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        void IrcTopic(string channel, string content);

        /// <summary>
        /// The IRC admin.
        /// </summary>
        void IrcAdmin();

        /// <summary>
        /// The IRC info.
        /// </summary>
        void IrcInfo();

        /// <summary>
        /// The IRC who.
        /// </summary>
        /// <param name="mask">
        /// The mask.
        /// </param>
        void IrcWho(string mask);

        /// <summary>
        /// The IRC whois.
        /// </summary>
        /// <param name="mask">
        /// The mask.
        /// </param>
        void IrcWhois(string mask);

        /// <summary>
        /// The IRC who was.
        /// </summary>
        /// <param name="mask">
        /// The mask.
        /// </param>
        void IrcWhowas(string mask);

        /// <summary>
        /// The IRC kill.
        /// </summary>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="comment">
        /// The comment.
        /// </param>
        void IrcKill(string nickname, string comment);

        /// <summary>
        /// The IRC away.
        /// </summary>
        void IrcAway();

        /// <summary>
        /// The IRC away.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void IrcAway(string message);

        /// <summary>
        /// The IRC is on.
        /// </summary>
        /// <param name="nicklist">
        /// The nick list.
        /// </param>
        void IrcIson(string nicklist);

        /// <summary>
        ///   Compares the channel name against the valid channel name settings returned by the IRC server on connection
        /// </summary>
        /// <param name = "channelName">Channel name to check</param>
        /// <returns>Boolean true if provided channel name is valid</returns>
        bool IsValidChannelName(string channelName);

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

        /// <summary>
        /// The Stop.
        /// </summary>
        void Stop();

        /// <summary>
        /// The register instance.
        /// </summary>
        void RegisterInstance();

        /// <summary>
        /// The get thread status.
        /// </summary>
        /// <returns>
        /// The string array
        /// </returns>
        string[] GetThreadStatus();
    }
}