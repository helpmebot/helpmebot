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
            this.realClient.JoinReceivedEvent += this.OnJoinReceived;
            this.realClient.InviteReceivedEvent += this.OnInviteReceived;

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
        public event EventHandler<JoinEventArgs> JoinEvent;

        /// <summary>
        /// The invite event.
        /// </summary>
        public event EventHandler<InviteEventArgs> InviteEvent;

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
            if (e.Message.Command == "PRIVMSG")
            {
                EventHandler<PrivateMessageEventArgs> privateMessageEvent = this.PrivateMessageEvent;
                if (privateMessageEvent != null)
                {
                    var parameters = e.Message.Parameters.ToList();
                    privateMessageEvent(this, new PrivateMessageEventArgs(LegacyUser.NewFromString(e.Message.Prefix), parameters[0], parameters[1]));
                }
            }

            if (e.Message.Command == "NOTICE")
            {
                EventHandler<PrivateMessageEventArgs> noticeEvent = this.NoticeEvent;
                if (noticeEvent != null)
                {
                    var parameters = e.Message.Parameters.ToList();
                    noticeEvent(this, new PrivateMessageEventArgs(LegacyUser.NewFromString(e.Message.Prefix), parameters[0], parameters[1]));
                }
            }
        }

        /// <summary>
        /// The on join received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnJoinReceived(object sender, JoinEventArgs e)
        {
            EventHandler<JoinEventArgs> temp = this.JoinEvent;
            if (temp != null)
            {
                temp(sender, e);
            }
        }

        /// <summary>
        /// The on invite received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnInviteReceived(object sender, InviteEventArgs e)
        {
            EventHandler<InviteEventArgs> inviteEventHandler = this.InviteEvent;
            if (inviteEventHandler != null)
            {
                inviteEventHandler(sender, e);
            }
        }
    }
}
