// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyIrcProxy.cs" company="Helpmebot Development Team">
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
//   Defines the LegacyIrcProxy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Legacy.IRC
{
    using System;
    using System.Linq;

    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.IRC.Messages;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Threading;

    /// <summary>
    /// The legacy IRC proxy.
    /// </summary>
    public class LegacyIrcProxy : IIrcAccessLayer, IThreadedSystem
    {
        /// <summary>
        /// The real client.
        /// </summary>
        private readonly IIrcClient realClient;

        /// <summary>
        /// Initialises a new instance of the <see cref="LegacyIrcProxy"/> class.
        /// </summary>
        /// <param name="realClient">
        /// The real client.
        /// </param>
        public LegacyIrcProxy(IIrcClient realClient)
        {
            this.realClient = realClient;
            this.realClient.ReceivedMessage += this.OnReceivedMessage;

            var temp = this.ConnectionRegistrationSucceededEvent;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The connection registration succeeded event.
        /// </summary>
        public event EventHandler ConnectionRegistrationSucceededEvent;

        /// <summary>
        /// The join event.
        /// </summary>
        public event IrcAccessLayer.JoinEventHandler JoinEvent;

        /// <summary>
        /// The invite event.
        /// </summary>
        public event IrcAccessLayer.InviteEventHandler InviteEvent;

        /// <summary>
        /// The private message event.
        /// </summary>
        public event EventHandler<PrivateMessageEventArgs> PrivateMessageEvent;

        /// <summary>
        /// The notice event.
        /// </summary>
        public event EventHandler<PrivateMessageEventArgs> NoticeEvent;

        /// <summary>
        /// The thread fatal error event.
        /// </summary>
        public event EventHandler ThreadFatalErrorEvent;

        /// <summary>
        /// Gets the nickname.
        /// </summary>
        public string Nickname
        {
            get
            {
                return this.realClient.Nickname;
            }
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public void Stop()
        {
            this.realClient.Send(new Message("QUIT"));
        }

        /// <summary>
        /// The register instance.
        /// </summary>
        public void RegisterInstance()
        {
            ThreadList.GetInstance().Register(this);
        }

        /// <summary>
        /// The get thread status.
        /// </summary>
        /// <returns>
        /// The status.
        /// </returns>
        public string[] GetThreadStatus()
        {
            return new[] { "IrcProxy - UNKNOWN" };
        }

        /// <summary>
        /// The connect.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Connect()
        {
            return true;
        }

        /// <summary>
        /// The send raw line.
        /// </summary>
        /// <param name="line">
        /// The line.
        /// </param>
        public void SendRawLine(string line)
        {
            this.realClient.Send(Message.Parse(line));
        }

        /// <summary>
        /// The IRC PRIVMSG.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public void IrcPrivmsg(string destination, string message)
        {
            this.realClient.SendMessage(destination, message);
        }

        /// <summary>
        /// The IRC join.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void IrcJoin(string channel)
        {
            this.realClient.JoinChannel(channel);
        }

        /// <summary>
        /// The IRC part.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public void IrcPart(string channel, string message)
        {
            this.realClient.PartChannel(channel, message);
        }

        /// <summary>
        /// The IRC notice.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public void IrcNotice(string destination, string message)
        {
            this.realClient.Send(new Message("NOTICE", new[] { destination, message }));
        }

        /// <summary>
        /// The on received message.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnReceivedMessage(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Command == "JOIN")
            {
                IrcAccessLayer.JoinEventHandler joinEventHandler = this.JoinEvent;
                if (joinEventHandler != null)
                {
                    joinEventHandler(LegacyUser.newFromString(e.Message.Prefix), e.Message.Parameters.First());
                }
            }

            if (e.Message.Command == "INVITE")
            {
                IrcAccessLayer.InviteEventHandler inviteEventHandler = this.InviteEvent;
                if (inviteEventHandler != null)
                {
                    var parameters = e.Message.Parameters.ToList();
                    inviteEventHandler(LegacyUser.newFromString(e.Message.Prefix), parameters[0], parameters[1]);
                }
            }

            if (e.Message.Command == "PRIVMSG")
            {
                EventHandler<PrivateMessageEventArgs> privateMessageEvent = this.PrivateMessageEvent;
                if (privateMessageEvent != null)
                {
                    var parameters = e.Message.Parameters.ToList();
                    privateMessageEvent(this, new PrivateMessageEventArgs(LegacyUser.newFromString(e.Message.Prefix), parameters[0], parameters[1]));
                }
            }

            if (e.Message.Command == "NOTICE")
            {
                EventHandler<PrivateMessageEventArgs> noticeEvent = this.NoticeEvent;
                if (noticeEvent != null)
                {
                    var parameters = e.Message.Parameters.ToList();
                    noticeEvent(this, new PrivateMessageEventArgs(LegacyUser.newFromString(e.Message.Prefix), parameters[0], parameters[1]));
                }
            }
        }
    }
}
