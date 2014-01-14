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
// <remarks>
// TODO: +Q user mode for no forwarding.
// TODO: automatic CTCP replies
// TODO: kick tracking
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.IRC.Messages;
    using Helpmebot.IRC.Model;
    using Helpmebot.Model.Interfaces;

    using NHibernate.Proxy;

    using IMessage = Helpmebot.IRC.Messages.IMessage;

    /// <summary>
    /// The IRC client.
    /// </summary>
    public class IrcClient : IIrcClient, IDisposable
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
        /// The sync logger.
        /// </summary>
        private readonly ILogger syncLogger;

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
        /// The lock object for operations on the user/channel lists.
        /// </summary>
        private readonly object userOperationLock = new object();

        /// <summary>
        /// The cap extended join.
        /// </summary>
        private bool capExtendedJoin;

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
        private bool servicesLoggedIn;

        /// <summary>
        /// The server prefix.
        /// </summary>
        private string serverPrefix;

        /// <summary>
        /// The nick tracking valid.
        /// </summary>
        private bool nickTrackingValid = true;

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
            this.syncLogger = logger.CreateChildLogger("Sync");
            this.username = username;
            this.realName = realName;
            this.password = password;
            this.networkClient.DataReceived += this.NetworkClientOnDataReceived;
            this.ReceivedMessage += this.OnMessageReceivedEvent;

            this.clientCapabilities = new List<string> { "sasl", "account-notify", "extended-join", "multi-prefix" };

            this.userCache = new Dictionary<string, IrcUser>();
            this.channels = new Dictionary<string, Channel>();

            this.connectionRegistrationSemaphore = new Semaphore(0, 1);
            this.syncLogger.Debug("ctor() acquired connectionRegistration semaphore.");

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
                this.Send(new Message("NICK", value));
            }
        }

        /// <summary>
        /// Gets the channels.
        /// </summary>
        public Dictionary<string, Channel> Channels
        {
            get
            {
                return this.channels;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the nick tracking is valid.
        /// </summary>
        public bool NickTrackingValid
        {
            get
            {
                return this.nickTrackingValid;
            }
        }

        /// <summary>
        /// Gets the user cache.
        /// </summary>
        public Dictionary<string, IrcUser> UserCache
        {
            get
            {
                return this.userCache;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the client logged in to a nickserv account
        /// </summary>
        public bool ServicesLoggedIn
        {
            get
            {
                return this.servicesLoggedIn;
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
        public void JoinChannel(string channel)
        {
            if (channel == "0")
            {
                throw new SecurityException("Not allowed to part all with JOIN 0");
            }

            this.syncLogger.DebugFormat("Join({0}) waiting on connectionRegistration semaphore.", channel);
            this.connectionRegistrationSemaphore.WaitOne();
            this.syncLogger.DebugFormat("Join({0}) acquired on connectionRegistration semaphore.", channel);
            this.connectionRegistrationSemaphore.Release();
            this.syncLogger.DebugFormat("Join({0}) released connectionRegistration semaphore.", channel);
            
            // request to join
            this.Send(new Message("JOIN", channel));
        }

        /// <summary>
        /// The part channel.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public void PartChannel(string channel, string message)
        {
            // request to join
            this.Send(new Message("PART", new[] { channel, message }));
        }

        /// <summary>
        /// The send message.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public void SendMessage(string destination, string message)
        {
            this.Send(new Message("PRIVMSG", new[] { destination, message }));
        }

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Send(IMessage message)
        {
            this.networkClient.Send(message.ToString());
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.networkClient.Disconnect();
                this.networkClient.Dispose();
                ((IDisposable)this.connectionRegistrationSemaphore).Dispose();
            }
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
                this.Send(new Message("PONG", message.Parameters));
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
            EventHandler<MessageReceivedEventArgs> receivedMessageEvent = this.ReceivedMessage;
            if (receivedMessageEvent != null)
            {
                receivedMessageEvent(this, new MessageReceivedEventArgs(message));
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
                this.OnJoinReceived(e, user);
            }

            if (e.Message.Command == Numerics.NameReply)
            {
                this.OnNameReplyReceived(e);
            }

            if (e.Message.Command == Numerics.WhoXReply)
            {
                this.logger.DebugFormat("WHOX Reply:{0}", e.Message.Parameters.Implode());
                this.HandleWhoXReply(e.Message);
            }

            if (e.Message.Command == Numerics.EndOfWho)
            {
                this.logger.Debug("End of who list.");
            }

            if (e.Message.Command == "QUIT" && user != null)
            {
                this.OnQuitMessageReceived(user);
            }

            if (e.Message.Command == "MODE" && user != null)
            {
                var parameters = e.Message.Parameters.ToList();
                var target = parameters[0];
                if (target.StartsWith("#"))
                {
                    this.OnChannelModeReceived(parameters);
                }
                else
                {
                    // User mode message
                    this.logger.Debug("Received user mode message. Not processing.");
                }
            }

            if (e.Message.Command == "PART" && user != null)
            {
                this.OnPartMessageReceived(e, user);
            }

            if (e.Message.Command == "ACCOUNT" && user != null)
            {
                this.OnAccountMessageReceived(e, user);
            }

            if (e.Message.Command == "NICK" && user != null)
            {
                this.OnNickChangeReceived(e, user);
            }
        }

        #region Message received event handlers

        /// <summary>
        /// The on account message received.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        private void OnAccountMessageReceived(MessageReceivedEventArgs e, IUser user)
        {
            var parameters = e.Message.Parameters.ToList();

            lock (this.userOperationLock)
            {
                this.logger.DebugFormat("Seen {0} change account name to {1}", user, parameters[0]);
                if (this.UserCache.ContainsKey(user.Nickname))
                {
                    this.UserCache[user.Nickname].Account = parameters[0];
                }
                else
                {
                    this.UserCache.Add(user.Nickname, (IrcUser)user);
                    user.Account = parameters[0];
                }
            }
        }

        /// <summary>
        /// The on part message received.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        private void OnPartMessageReceived(MessageReceivedEventArgs e, IUser user)
        {
            var parameters = e.Message.Parameters.ToList();
            var channel = parameters[0];
            if (user.Nickname == this.Nickname)
            {
                this.logger.InfoFormat("Leaving channel {1}.", user, channel);

                lock (this.userOperationLock)
                {
                    this.channels.Remove(channel);
                }
            }
            else
            {
                lock (this.userOperationLock)
                {
                    this.channels[channel].Users.Remove(user.Nickname);
                }

                this.logger.InfoFormat("{0} has left channel {1}.", user, channel);
            }
        }

        /// <summary>
        /// The on quit message received.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        private void OnQuitMessageReceived(IUser user)
        {
            this.logger.InfoFormat("{0} has left IRC.", user);

            lock (this.userOperationLock)
            {
                this.UserCache.Remove(user.Nickname);

                foreach (var channel in this.channels)
                {
                    channel.Value.Users.Remove(user.Nickname);
                }
            }
        }

        /// <summary>
        /// The on nick change received.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        private void OnNickChangeReceived(MessageReceivedEventArgs e, IUser user)
        {
            var parameters = e.Message.Parameters.ToList();
            var newNickname = parameters[0];
            var oldNickname = user.Nickname;
            lock (this.userOperationLock)
            {
                // firstly, update the user cache.
                var ircUser = this.UserCache[oldNickname];
                this.UserCache.Remove(oldNickname);

                ircUser.Nickname = newNickname;

                this.UserCache.Add(newNickname, ircUser);

                // secondly, update the channels this user is in.
                foreach (KeyValuePair<string, Channel> channelPair in this.channels)
                {
                    if (channelPair.Value.Users.ContainsKey(oldNickname))
                    {
                        var channelUser = channelPair.Value.Users[oldNickname];

                        if (!channelUser.User.Equals(ircUser))
                        {
                            this.logger.ErrorFormat(
                                "Channel user {0} doesn't match irc user {1} for NICK in {2}",
                                channelUser.User,
                                ircUser,
                                channelPair.Value.Name);

                            this.logger.Error("Nick tracking is no longer valid.");
                            this.nickTrackingValid = false;

                            throw new Exception("Channel user doesn't match irc user");
                        }

                        channelPair.Value.Users.Remove(oldNickname);
                        channelPair.Value.Users.Add(newNickname, channelUser);
                    }
                }
            }
        }

        /// <summary>
        /// The on name reply received.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnNameReplyReceived(MessageReceivedEventArgs e)
        {
            var parameters = e.Message.Parameters.ToList();

            var channel = parameters[2];
            var names = parameters[3];

            this.logger.DebugFormat("Names on {0}: {1}", channel, names);
            
            foreach (string name in names.Split(' '))
            {
                var parsedName = name;
                var voice = false;
                var op = false;

                if (parsedName.StartsWith("+"))
                {
                    parsedName = parsedName.Substring(1);
                    voice = true;
                }

                if (parsedName.StartsWith("@"))
                {
                    parsedName = parsedName.Substring(1);
                    op = true;
                }

                lock (this.userOperationLock)
                {
                    if (this.channels[channel].Users.ContainsKey(parsedName))
                    {
                        var channelUser = this.channels[channel].Users[parsedName];
                        channelUser.Operator = op;
                        channelUser.Voice = voice;
                    }
                    else
                    {
                        var ircUser = new IrcUser { Nickname = parsedName };
                        if (this.UserCache.ContainsKey(parsedName))
                        {
                            ircUser = this.UserCache[parsedName];
                        }
                        else
                        {
                            this.UserCache.Add(parsedName, ircUser);
                        }

                        var channelUser = new ChannelUser(ircUser, channel) { Voice = voice, Operator = op };

                        this.channels[channel].Users.Add(parsedName, channelUser);
                    }
                }
            }
        }

        /// <summary>
        /// The on channel mode received.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        private void OnChannelModeReceived(List<string> parameters)
        {
            // Channel Mode message
            var channel = parameters[0];
            var modechange = parameters[1];

            var addMode = true;
            var position = 2;

            foreach (char c in modechange)
            {
                if (c == '-')
                {
                    addMode = false;
                }

                if (c == '+')
                {
                    addMode = true;
                }

                if (c == 'o')
                {
                    var nick = parameters[position];

                    lock (this.userOperationLock)
                    {
                        var channelUser = this.channels[channel].Users[nick];
                     
                        this.logger.InfoFormat("Seen {0}o on {1}.", addMode ? "+" : "-", channelUser);

                        channelUser.Operator = addMode;

                        position++;
                    }
                }

                if (c == 'v')
                {
                    var nick = parameters[position];

                    lock (this.userOperationLock)
                    {
                        var channelUser = this.channels[channel].Users[nick];

                        this.logger.InfoFormat("Seen {0}v on {1}.", addMode ? "+" : "-", channelUser, channel);

                        channelUser.Voice = addMode;

                        position++;
                    }
                }

                if ("eIbqkflj".Contains(c))
                {
                    position++;
                }
            }
        }

        /// <summary>
        /// The on join received.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        private void OnJoinReceived(MessageReceivedEventArgs e, IUser user)
        {
            // this is a client join to a channel.
            // :stwalkerster!stwalkerst@wikimedia/stwalkerster JOIN ##stwalkerster
            var parametersList = e.Message.Parameters.ToList();

            lock (this.userOperationLock)
            {
                if (this.userCache.ContainsKey(user.Nickname))
                {
                    user = this.userCache[user.Nickname];
                }
                else
                {
                    this.userCache.Add(user.Nickname, (IrcUser)user);
                }
            }

            if (this.capExtendedJoin)
            {
                // :stwalkerster!stwalkerst@wikimedia/stwalkerster JOIN ##stwalkerster accountname :realname
                user.Account = parametersList[1];
            }

            var channelName = parametersList[0];
            if (user.Nickname == this.Nickname)
            {
                // we're joining this, so rate-limit from here.
                this.logger.InfoFormat("Joining channel {0}", channelName);
                this.logger.Debug("Requesting WHOX a information");
                this.Send(new Message("WHO", new[] { channelName, "%uhnatfc,001" }));

                lock (this.userOperationLock)
                {
                    // add the channel to the list of channels I'm in.
                    this.Channels.Add(channelName, new Channel(channelName));
                }
            }
            else
            {
                this.logger.InfoFormat("Seen {0} join channel {1}.", user, channelName);

                lock (this.userOperationLock)
                {
                    this.Channels[channelName].Users.Add(user.Nickname, new ChannelUser((IrcUser)user, channelName));
                }
            }
        }

        #endregion

        /// <summary>
        /// The handle who x reply.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void HandleWhoXReply(IMessage message)
        {
            try
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
                var account = parameters[7];

                lock (this.userOperationLock)
                {
                    var ircUser = new IrcUser();
                    if (this.UserCache.ContainsKey(nick))
                    {
                        ircUser = this.UserCache[nick];
                    }
                    else
                    {
                        ircUser.Nickname = nick;
                        this.UserCache.Add(nick, ircUser);
                    }

                    ircUser.Account = account;
                    ircUser.Username = user;
                    ircUser.Hostname = host;
                    ircUser.Away = away;

                    if (this.channels[channel].Users.ContainsKey(ircUser.Nickname))
                    {
                        var channelUser = this.channels[channel].Users[ircUser.Nickname];
                        channelUser.Operator = modes.Contains("@");
                        channelUser.Voice = modes.Contains("+");
                    }
                    else
                    {
                        var channelUser = new ChannelUser(ircUser, channel)
                                              {
                                                  Operator = modes.Contains("@"),
                                                  Voice = modes.Contains("+")
                                              };

                        this.channels[channel].Users.Add(ircUser.Nickname, channelUser);
                    }
                }
            }
            catch (Exception ex)
            {
                this.nickTrackingValid = false;
                this.logger.Error("Nick tracking for authentication is no longer valid.", ex);
                throw;
            }
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

                    this.Send(new Message("CAP", "END"));
                    this.Send1459Registration();
                }
                else
                {
                    // we support capabilities, use them!
                    this.Send(new Message("CAP", "LS"));
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
                this.syncLogger.Debug("RegisterConnection() released connectionRegistration semaphore.");

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
                    this.logger.DebugFormat("Server Capabilities: {0}", serverCapabilities.Implode(", "));
                    this.logger.DebugFormat("Client Capabilities: {0}", this.clientCapabilities.Implode(", "));

                    var caps = serverCapabilities.Intersect(this.clientCapabilities).ToList();

                    // We don't support one without the other!
                    if (caps.Intersect(new[] { "account-notify", "extended-join" }).Count() == 1)
                    {
                        this.logger.Warn(
                            "Dropping account-notify and extended-join support since server only supports one of them!");
                        caps.Remove("account-notify");
                        caps.Remove("extended-join");
                    }

                    if (caps.Count == 0)
                    {
                        // nothing is suitable for us, so downgrade to 1459
                        this.logger.InfoFormat("Requesting no capabilities.");

                        this.Send(new Message("CAP", "END"));
                        this.Send1459Registration();

                        return;
                    }

                    this.logger.InfoFormat("Requesting capabilities: {0}", caps.Implode(", "));

                    this.Send(new Message("CAP", new[] { "REQ", caps.Implode() }));

                    return;
                }

                if (list[1] == "ACK")
                {
                    var caps = list[2].Split(' ');
                    this.logger.InfoFormat("Acknowledged capabilities: {0}", caps.Implode(", "));

                    foreach (var cap in caps)
                    {
                        if (cap == "sasl")
                        {
                            this.capSasl = true;
                        }

                        if (cap == "extended-join")
                        {
                            // This includes account-notify since both are required.
                            this.capExtendedJoin = true;
                        }

                        // We don't care about multi-prefix, since the code to 
                        // handle it works nicely for those without it.
                    }

                    if (this.capSasl)
                    {
                        this.SaslAuth(null);
                    }
                    else
                    {
                        this.Send(new Message("CAP", "END"));
                        this.Send1459Registration();  
                    }

                    return;
                }

                if (list[1] == "NAK")
                {
                    // something went wrong, so downgrade to 1459.
                    var caps = list[2].Split(' ');
                    this.logger.WarnFormat("NOT Acked capabilities: {0}", caps.Implode(", "));

                    this.Send(new Message("CAP", "END"));
                    this.Send1459Registration();
                    return;
                }
            }

            if (message.Command == Numerics.SaslLoggedIn)
            {
                var strings = message.Parameters.ToArray();
                this.logger.InfoFormat("You are now logged in as {1} ({0})", strings[1], strings[2]);
                this.servicesLoggedIn = true;
                return;
            }

            if (message.Command == Numerics.SaslSuccess)
            {
                this.logger.InfoFormat("SASL Login succeeded.");

                // logged in, continue with registration
                this.Send(new Message("CAP", "END"));
                this.Send1459Registration();
                return;
            }

            if (message.Command == Numerics.SaslAuthFailed)
            {
                this.logger.Fatal("SASL Login failed.");

                // not logged in, cancel sasl auth.
                this.Send(new Message("QUIT"));
                return;
            }

            if (message.Command == Numerics.SaslAborted)
            {
                this.logger.WarnFormat("SASL Login aborted.");

                // not logged in, cancel sasl auth.
                this.Send(new Message("CAP", "END"));
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
                this.Send(new Message("AUTHENTICATE", "PLAIN"));
                return;
            }

            var list = message.Parameters.ToList();
            if (list[0] == "+")
            {
                var authdata = string.Format("\0{0}\0{1}", this.username, this.password).ToBase64();
                this.Send(new Message("AUTHENTICATE", authdata));
            }
        }

        /// <summary>
        /// The send 1459 registration.
        /// </summary>
        private void Send1459Registration()
        {
            if (!this.capSasl && !string.IsNullOrEmpty(this.password))
            {
                this.Send(new Message("PASS", this.password));
            }

            this.Send(new Message("USER", new[] { this.username, "*", "*", this.realName }));

            this.Send(new Message("NICK", this.nickname));
        }

        #endregion
    }
}
