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
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.IRC.Messages;

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

            this.clientCapabilities = new List<string> { "sasl"/*, "account-notify", "extended-join", "multi-prefix"*/ };

            this.RegisterConnection(null);
        }

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
        }

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

            // welcome to IRC!
            if (message.Command == Numerics.Welcome)
            {
                this.logger.Info("Connection registration succeeded");
                this.connectionRegistered = true;
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
                this.logger.InfoFormat("SASL Login succeeded.");

                // logged in, continue with registration
                this.Send(new Message { Command = "CAP", Parameters = new List<string> { "END" } });
                this.Send1459Registration();
                return;
            }

            if (message.Command == Numerics.SaslAuthFailed)
            {
                this.logger.WarnFormat("SASL Login failed.");

                this.capSasl = false;

                // not logged in, continue with registration
                this.Send(new Message { Command = "CAP", Parameters = new List<string> { "END" } });
                this.Send1459Registration();
                return;
            }
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
    }
}
