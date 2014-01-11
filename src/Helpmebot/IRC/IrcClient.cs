// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcClient.cs" company="Helpmebot Development Team">
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
//   Defines the IrcClient type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.IRC.Messages;
    using Helpmebot.IRC.Model;
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The IRC client.
    /// </summary>
    public class IrcClient
    {
        #region Fields

        /// <summary>
        /// The network client.
        /// </summary>
        private readonly INetworkClient networkClient;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The username.
        /// </summary>
        private readonly string username;

        /// <summary>
        /// The real name.
        /// </summary>
        private readonly string realName;

        /// <summary>
        /// The password.
        /// </summary>
        private readonly string password;

        /// <summary>
        /// The client's possible capabilities.
        /// </summary>
        private readonly List<string> clientCapabilities;

        /// <summary>
        /// The user cache.
        /// </summary>
        private readonly Dictionary<string, IrcUser> userCache;

        /// <summary>
        /// The channels.
        /// </summary>
        private readonly Dictionary<string, Channel> channels;

        /// <summary>
        /// The connection registration semaphore.
        /// </summary>
        private readonly Semaphore connectionRegistrationSemaphore;

        /// <summary>
        /// The cap multi prefix.
        /// </summary>
        private bool capMultiPrefix;

        /// <summary>
        /// The cap extended join.
        /// </summary>
        private bool capExtendedJoin;

        /// <summary>
        /// The cap account notify.
        /// </summary>
        private bool capAccountNotify;

        /// <summary>
        /// The SASL capability.
        /// </summary>
        private bool capSasl;

        /// <summary>
        /// The data interception function.
        /// </summary>
        private bool connectionRegistered;

        /// <summary>
        /// The nickname.
        /// </summary>
        private string nickname;

        /// <summary>
        /// Is the client logged in to a nickserv account?
        /// </summary>
        private bool loggedIn;

        /// <summary>
        /// The server prefix.
        /// </summary>
        private string serverPrefix;

        #endregion

        /// <summary>
        /// Initialises a new instance of the <see cref="IrcClient"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="realName">
        /// The real Name.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        public IrcClient(INetworkClient client, ILogger logger, string nickname, string username, string realName, string password)
        {
            this.nickname = nickname;
            this.networkClient = client;
            this.logger = logger;
            this.username = username;
            this.realName = realName;
            this.password = password;
            this.networkClient.DataReceived += this.NetworkClientOnDataReceived;
            this.ReceivedMessage += this.OnMessageReceivedEvent;

            this.clientCapabilities = new List<string> { "sasl", "account-notify", "extended-join", "multi-prefix" };

            this.userCache = new Dictionary<string, IrcUser>();
            this.channels = new Dictionary<string, Channel>();

            this.connectionRegistrationSemaphore = new Semaphore(0, 1);

            this.RegisterConnection(null);
        }

        /// <summary>
        /// The received message.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> ReceivedMessage;

        #region Properties

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public string Nickname
        {
            get
            {
                return this.nickname;
            }

            set
            {
                this.nickname = value;
                this.Send(new Message { Command = "NICK", Parameters = value.ToEnumerable() });
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// The join.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void Join(string channel)
        {
            if (channel == "0")
            {
                return;
            }

            this.connectionRegistrationSemaphore.WaitOne();

            this.Send(new Message { Command = "JOIN", Parameters = channel.ToEnumerable() });
        }

        #endregion

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void Send(Message message)
        {
            this.networkClient.Send(message.ToString());
        }

        /// <summary>
        /// The network client on data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="dataReceivedEventArgs">
        /// The data received event args.
        /// </param>
        private void NetworkClientOnDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            var message = Message.Parse(dataReceivedEventArgs.Data);

            if (message.Command == "ERROR")
            {
                var errorMessage = message.Parameters.First();
                this.logger.Fatal(errorMessage);
                this.networkClient.Disconnect();
                throw new IOException(errorMessage);
            }

            if (message.Command == "PING")
            {
                this.Send(new Message { Command = "PONG", Parameters = message.Parameters });
            }

            if (this.connectionRegistered)
            {
                this.RaiseDataEvent(message);
            }
            else
            {
                this.RegisterConnection(message);
            }
        }

        /// <summary>
        /// The raise data event.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void RaiseDataEvent(IMessage message)
        {
            EventHandler<MessageReceivedEventArgs> temp = this.ReceivedMessage;
            if (temp != null)
            {
                temp(this, new MessageReceivedEventArgs(message));
            }
        }

        /// <summary>
        /// The on message received event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnMessageReceivedEvent(object sender, MessageReceivedEventArgs e)
        {
            IUser user = null;
            if (e.Message.Prefix != null)
            {
                if (e.Message.Prefix == this.serverPrefix)
                {
                    user = new ServerUser();
                }
                else
                {
                    user = IrcUser.FromPrefix(e.Message.Prefix);
                }
            }

            if (e.Message.Command == "JOIN" && user != null)
            {
                // this is a client join to a channel.
                // :stwalkerster!stwalkerst@wikimedia/stwalkerster JOIN ##stwalkerster
                var parametersList = e.Message.Parameters.ToList();
                
                if (this.capExtendedJoin)
                {
                    // :stwalkerster!stwalkerst@wikimedia/stwalkerster JOIN ##stwalkerster accountname :realname
                    user.Account = parametersList[1];
                }

                var channelName = parametersList[0];
                if (user.Nickname == this.Nickname)
                {
                    this.logger.InfoFormat("Joining channel {0}", channelName);
                    this.logger.Debug("Requesting WHOX a information");
                    this.Send(new Message { Command = "WHO", Parameters = new[] { channelName, "%uhnatfc,001" } });

                    // add the channel to the list of channels I'm in.
                    this.channels.Add(channelName, new Channel(channelName));
                }
                else
                {
                    this.logger.DebugFormat("Seen {0} join channel {1}.", user, channelName);
                    this.userCache.AddOrReplace(user.Nickname, (IrcUser)user);
                    this.channels[channelName].Users.Add(user.Nickname, new ChannelUser((IrcUser)user, channelName));
                }
            }

            if (e.Message.Command == Numerics.WhoXReply)
            {
                this.logger.WarnFormat("WHOX Reply:{0}", e.Message.Parameters.Implode());
                this.HandleWhoXReply(e.Message);
            }

            if (e.Message.Command == Numerics.EndOfWho)
            {
                this.logger.Debug("End of who list.");
                this.connectionRegistrationSemaphore.Release();
            }
        }

        /// <summary>
        /// The handle who x reply.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void HandleWhoXReply(IMessage message)
        {
            if (message.Command != Numerics.WhoXReply)
            {
                throw new ArgumentException("Expected WHOX reply message", "message");
            }

            var parameters = message.Parameters.ToList();
            if (parameters.Count() != 8)
            {
                throw new ArgumentException("Expected 8 WHOX parameters.", "message");
            }

            /* >> :holmes.freenode.net 354 stwalkerster 001 #wikipedia-en-accounts ChanServ services.           ChanServ       H@  0
             * >> :holmes.freenode.net 354 stwalkerster 001 #wikipedia-en-accounts ~jamesur wikimedia/Jamesofur Jamesofur|away G  jamesofur
             *                             .            t   c                      u        h                   n              f  a
             *     prefix              cmd    0         1   2                      3        4                   5              6  7
             */
            var channel = parameters[2];
            var user = parameters[3];
            var host = parameters[4];
            var nick = parameters[5];
            var flags = parameters[6];
            var away = flags[0] == 'G'; // H (here) / G (gone)
            var modes = flags.Substring(1);
            var account = parameters[7] == "0" ? "*" : parameters[7];

            var ircUser = new IrcUser();
            if (this.userCache.ContainsKey(nick))
            {
                ircUser = this.userCache[nick];
            }
            else
            {
                ircUser.Nickname = nick;
                this.userCache.Add(nick, ircUser);
            }

            ircUser.Account = account;
            ircUser.Username = user;
            ircUser.Hostname = host;
            ircUser.Away = away;

         // TODO:   this.channels[channel].Users.Add();
        }

        #region Connection Registration

        /// <summary>
        /// The register connection.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void RegisterConnection(IMessage message)
        {
            // initial request
            if (message == null)
            {
                if (this.clientCapabilities.Count == 0)
                {
                    // we don't support capabilities, so don't go through the CAP cycle.
                    this.logger.InfoFormat("I support no capabilities.");

                    this.Send(new Message { Command = "CAP", Parameters = new List<string> { "END" } });
                    this.Send1459Registration();
                }
                else
                {
                    // we support capabilities, use them!
                    this.Send(new Message { Command = "CAP", Parameters = new List<string> { "LS" } });    
                }
                
                return;
            }

            if (message.Command == "NOTICE")
            {
                // do nothing, we don't care about these messages during registration.
                return;
            }

            // welcome to IRC!
            if (message.Command == Numerics.Welcome)
            {
                this.logger.Info("Connection registration succeeded");
                this.serverPrefix = message.Prefix;
                this.connectionRegistered = true;
                this.connectionRegistrationSemaphore.Release();

                this.RaiseDataEvent(message);
                return;
            }

            // nickname in use
            if (message.Command == Numerics.NicknameInUse)
            {
                this.logger.Warn("Nickname in use, retrying.");
                this.Nickname = this.Nickname + "_";
                return;
            }

            // do sasl auth
            if (message.Command == "AUTHENTICATE")
            {
                this.SaslAuth(message);
                return;
            }

            // we've recieved a reply to our CAP commands
            if (message.Command == "CAP")
            {
                var list = message.Parameters.ToList();
                
                if (list[1] == "LS")
                {
                    var serverCapabilities = list[2].Split(' ');
                    this.logger.InfoFormat("Server Capabilities: {0}", serverCapabilities.Implode(", "));
                    this.logger.InfoFormat("Client Capabilities: {0}", this.clientCapabilities.Implode(", "));

                    var caps = serverCapabilities.Intersect(this.clientCapabilities).ToList();

                    if (caps.Count == 0)
                    {
                        // nothing is suitable for us, so downgrade to 1459
                        this.logger.InfoFormat("Requesting no capabilities.");

                        this.Send(new Message { Command = "CAP", Parameters = new List<string> { "END" } });
                        this.Send1459Registration();

                        return;
                    }

                    this.logger.InfoFormat("Requesting capabilities: {0}", caps.Implode(", "));

                    this.Send(
                        new Message { Command = "CAP", Parameters = new List<string> { "REQ", caps.Implode() } });

                    return;
                }

                if (list[1] == "ACK")
                {
                    var caps = list[2].Split(' ');
                    this.logger.InfoFormat("Acknowledged capabilities: {0}", caps.Implode(", "));

                    foreach (var cap in caps)
                    {
                        if (cap == "account-notify")
                        {
                            this.capAccountNotify = true;
                        }

                        if (cap == "sasl")
                        {
                            this.capSasl = true;
                        }

                        if (cap == "extended-join")
                        {
                            this.capExtendedJoin = true;
                        }

                        if (cap == "multi-prefix")
                        {
                            this.capMultiPrefix = true;
                        }
                    }

                    if (this.capSasl)
                    {
                        this.SaslAuth(null);
                    }
                    else
                    {
                        this.Send(new Message { Command = "CAP", Parameters = new List<string> { "END" } });
                        this.Send1459Registration();  
                    }

                    return;
                }

                if (list[1] == "NAK")
                {
                    // something went wrong, so downgrade to 1459.
                    var caps = list[2].Split(' ');
                    this.logger.WarnFormat("NOT Acked capabilities: {0}", caps.Implode(", "));
                    
                    this.Send(new Message { Command = "CAP", Parameters = new List<string> { "END" } });
                    this.Send1459Registration();
                    return;
                }
            }

            if (message.Command == Numerics.SaslLoggedIn)
            {
                this.logger.InfoFormat("You are now logged in as {2} ({1})", message.Parameters.ToArray());
                this.loggedIn = true;
                return;
            }

            if (message.Command == Numerics.SaslSuccess)
            {
                this.logger.InfoFormat("SASL Login succeeded.");

                // logged in, continue with registration
                this.Send(new Message { Command = "CAP", Parameters = new List<string> { "END" } });
                this.Send1459Registration();
                return;
            }

            if (message.Command == Numerics.SaslAuthFailed)
            {
                this.logger.WarnFormat("SASL Login failed.");

                // not logged in, cancel sasl auth.
                this.Send(new Message { Command = "AUTHENTICATE", Parameters = "*".ToEnumerable() });
                return;
            }

            if (message.Command == Numerics.SaslAborted)
            {
                this.logger.WarnFormat("SASL Login aborted.");

                // not logged in, cancel sasl auth.
                this.Send(new Message { Command = "CAP", Parameters = "END".ToEnumerable() });
                this.Send1459Registration();
                return;
            }

            this.logger.Error("How did I get here?");
        }

        /// <summary>
        /// The SASL authentication.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void SaslAuth(IMessage message)
        {
            if (message == null)
            {
                this.Send(new Message { Command = "AUTHENTICATE", Parameters = "PLAIN".ToEnumerable() });
                return;
            }

            var list = message.Parameters.ToList();
            if (list[0] == "+")
            {
                var authdata = string.Format("\0{0}\0{1}", this.username, this.password).ToBase64();
                this.Send(new Message { Command = "AUTHENTICATE", Parameters = authdata.ToEnumerable() });
            }
        }

        /// <summary>
        /// The send 1459 registration.
        /// </summary>
        private void Send1459Registration()
        {
            if (!this.capSasl && !string.IsNullOrEmpty(this.password))
            {
                this.Send(new Message { Command = "PASS", Parameters = this.password.ToEnumerable() });
            }

            this.Send(
                new Message
                    {
                        Command = "USER",
                        Parameters = new List<string> { this.username, "*", "*", this.realName }
                    });

            this.Send(new Message { Command = "NICK", Parameters = this.Nickname.ToEnumerable() });
        }

        #endregion
    }
}
