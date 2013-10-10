// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcAccessLayer.cs" company="Helpmebot Development Team">
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
//   IRC Access Layer
//   Provides an interface to IRC.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using Helpmebot.IRC.Events;
    using Helpmebot.Threading;

    /// <summary>
    ///   IRC Access Layer - Provides an interface to IRC.
    /// </summary>
    public sealed class IrcAccessLayer : IThreadedSystem
    {
        #region Readonly Fields

        /// <summary>
        /// The names list.
        /// </summary>
        private readonly Hashtable namesList = new Hashtable();

        /// <summary>
        /// The last message.
        /// </summary>
        private readonly DateTime lastMessage = DateTime.Now;

        /// <summary>
        /// The network id.
        /// </summary>
        private readonly uint networkId;

        /// <summary>
        /// The channel list.
        /// </summary>
        private readonly ArrayList channelList = new ArrayList();

        /// <summary>
        /// The my nickname.
        /// </summary>
        private readonly string myNickname;

        /// <summary>
        /// The my username.
        /// </summary>
        private readonly string myUsername;

        /// <summary>
        /// The my real name.
        /// </summary>
        private readonly string myRealname;

        /// <summary>
        /// The my password.
        /// </summary>
        private readonly string myPassword;

        /// <summary>
        /// The nickserv.
        /// </summary>
        private readonly string nickserv;

        #endregion

        #region fields

        /// <summary>
        /// The IRC reader thread.
        /// </summary>
        private Thread ircReaderThread;

        /// <summary>
        /// The IRC writer thread.
        /// </summary>
        private Thread ircWriterThread;

        /// <summary>
        /// The TCP client.
        /// </summary>
        private TcpClient tcpClient;

        /// <summary>
        /// The IRC reader.
        /// </summary>
        private StreamReader ircReader;

        /// <summary>
        /// The IRC writer.
        /// </summary>
        private StreamWriter ircWriter;

        /// <summary>
        /// The send queue.
        /// </summary>
        private Queue sendQ;

        /// <summary>
        /// The my info reply.
        /// </summary>
        private string rplMyinfo;

        #endregion

        #region constructor/destructor

        /// <summary>
        /// Initialises a new instance of the <see cref="IrcAccessLayer"/> class.
        /// </summary>
        /// <param name="ircNetwork">
        /// The IRC network.
        /// </param>
        [Obsolete]
        public IrcAccessLayer(uint ircNetwork)
        {
            this.FloodProtectionWaitTime = 500;
            this.ClientVersion = "Helpmebot IRC Access Layer 1.0";
            this.networkId = ircNetwork; 

            DAL db = DAL.singleton();

            DAL.Select q = new DAL.Select(
                "in_host",
                "in_port",
                "in_nickname",
                "in_password",
                "in_username",
                "in_realname",
                "in_log",
                "in_nickserv");
            q.setFrom("ircnetwork");
            q.addLimit(1, 0);
            q.addWhere(new DAL.WhereConds("in_id", ircNetwork.ToString(CultureInfo.InvariantCulture)));

            ArrayList configSettings = db.executeSelect(q);

            this.Server = (string)((object[])configSettings[0])[0];
            this.Port = (uint)((object[])configSettings[0])[1];

            this.myNickname = (string)((object[])configSettings[0])[2];
            this.myPassword = (string)((object[])configSettings[0])[3];
            this.myUsername = (string)((object[])configSettings[0])[4];
            this.myRealname = (string)((object[])configSettings[0])[5];

            this.LogEvents = (bool)((object[])configSettings[0])[6];

            this.nickserv = (string)((object[])configSettings[0])[7];

            if (/*recieveWallops*/ true)
            {
                this.ConnectionUserModes += 4;
            }

            if (/*invisible*/ true)
            {
                this.ConnectionUserModes += 8;
            }

            this.InitialiseEventHandlers();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="IrcAccessLayer"/> class.
        /// </summary>
        /// <param name="server">
        /// The server.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="realname">
        /// The real name.
        /// </param>
        public IrcAccessLayer(string server, uint port, string nickname, string password, string username, string realname)
        {
            this.LogEvents = true;
            this.FloodProtectionWaitTime = 500;
            this.ClientVersion = "Helpmebot IRC Access Layer 1.0";
            this.networkId = 0;
            this.Server = server;
            this.Port = port;

            this.myNickname = nickname;
            this.myPassword = password;
            this.myUsername = username;
            this.myRealname = realname;

            this.InitialiseEventHandlers();
        }

        /// <summary>
        /// Finalises an instance of the <see cref="IrcAccessLayer"/> class. 
        /// </summary>
        ~IrcAccessLayer()
        {
            if (this.tcpClient.Connected)
            {
                this.IrcQuit();
            }
        }

        #endregion

        #region events

        /// <summary>
        /// The data received event.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceivedEvent;

        /// <summary>
        /// The unrecognised data received event.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> UnrecognisedDataReceivedEvent;

        public delegate void ConnectionRegistrationEventHandler();

        public event ConnectionRegistrationEventHandler connectionRegistrationSucceededEvent;

        public delegate void PingEventHandler(string datapacket);

        public event PingEventHandler pingEvent;

        public delegate void NicknameChangeEventHandler(string oldnick, string newnick);

        public event NicknameChangeEventHandler nicknameChangeEvent;

        public delegate void ModeChangeEventHandler(User source, string subject, string flagchanges, string parameter);

        public event ModeChangeEventHandler modeChangeEvent;

        public delegate void QuitEventHandler(User source, string message);

        public event QuitEventHandler quitEvent;

        public delegate void JoinEventHandler(User source, string channel);

        public event JoinEventHandler joinEvent;

        public delegate void PartEventHandler(User source, string channel, string message);

        public event PartEventHandler partEvent;

        public delegate void TopicEventHandler(User source, string channel, string topic);

        public event TopicEventHandler topicEvent;

        public delegate void InviteEventHandler(User source, string nickname, string channel);

        public event InviteEventHandler inviteEvent;

        public delegate void KickEventHandler(User source, string channel, string nick, string message);

        public event KickEventHandler kickEvent;

        public delegate void PrivmsgEventHandler(User source, string destination, string message);

        /// <summary>
        /// The private message event.
        /// </summary>
        public event EventHandler<PrivateMessageEventArgs> PrivateMessageEvent;

        /// <summary>
        /// The client to client event.
        /// </summary>
        public event EventHandler<PrivateMessageEventArgs> ClientToClientEvent;

        /// <summary>
        /// The notice event.
        /// </summary>
        public event EventHandler<PrivateMessageEventArgs> NoticeEvent;

        public delegate void IrcEventHandler();

        public event IrcEventHandler errNicknameInUseEvent;
        public event IrcEventHandler errUnavailResource;

        public delegate void NameReplyEventHandler(string channel, string[] names);

        // TODO: invoke this event somewhere
        public event NameReplyEventHandler nameReplyEvent;
        
        private event ConnectionRegistrationEventHandler connectionRegistrationRequiredEvent;
        #endregion

        #region properties

        /// <summary>
        /// Gets the server info.
        /// </summary>
        public string ServerInfo
        {
            get
            {
                return this.rplMyinfo;
            }
        }

        /// <summary>
        /// Gets or sets the client version.
        /// </summary>
        public string ClientVersion { get; set; }

        /// <summary>
        /// Gets a value indicating whether is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.tcpClient != null && this.tcpClient.Connected;
            }
        }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public string Nickname
        {
            get
            {
                return this.myNickname;
            }
            
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username
        {
            get { return this.myUsername; }
        }

        /// <summary>
        /// Gets the real name.
        /// </summary>
        public string RealName
        {
            get { return this.myRealname; }
        }

        /// <summary>
        /// Gets the server.
        /// </summary>
        public string Server { get; private set; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        public uint Port { get; private set; }

        /// <summary>
        /// Gets the my identity.
        /// </summary>
        /// <remarks>
        /// TODO: construct this dynamically?
        /// </remarks>
        public string MyIdentity
        {
            get { return this.Nickname + "!" + this.Username + "@wikimedia/bot/helpmebot"; }
        }

        /// <summary>
        /// Gets or sets the flood protection wait time.
        /// </summary>
        public int FloodProtectionWaitTime { get; set; }

        /// <summary>
        /// Gets the connection user modes.
        /// </summary>
        /// <remarks>
        /// +4 if receiving wallops, +8 if invisible
        /// </remarks>
        public int ConnectionUserModes { get; private set; }

        /// <summary>
        /// Gets the message count.
        /// </summary>
        public int MessageCount { get; private set; }

        /// <summary>
        /// Gets the idle time.
        /// </summary>
        public TimeSpan IdleTime
        {
            get { return DateTime.Now.Subtract(this.lastMessage); }
        }

        /// <summary>
        /// Gets a value indicating whether log events.
        /// </summary>
        public bool LogEvents { get; private set; }

        /// <summary>
        /// Gets the active channels.
        /// </summary>
        /// <remarks>
        /// TODO: check this actually behaves properly
        /// </remarks>
        public string[] ActiveChannels
        {
            get
            {
                Type t = Type.GetType("System.String");
                return (string[])this.channelList.ToArray(t);
            }
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// The wrap CTCP.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string WrapCTCP(string command, string parameters)
        {
            ASCIIEncoding asc = new ASCIIEncoding();
            byte[] ctcp = { Convert.ToByte(1) };
            return asc.GetString(ctcp) + command.ToUpper()
                    + (parameters == string.Empty ? string.Empty : " " + parameters) + asc.GetString(ctcp);
        }

        /// <summary>
        /// The connect.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Connect()
        {
            try
            {
                this.tcpClient = new TcpClient(this.Server, (int)this.Port);

                Stream ircStream = this.tcpClient.GetStream();
                this.ircReader = new StreamReader(ircStream, Encoding.UTF8);
                this.ircWriter = new StreamWriter(ircStream, Encoding.UTF8);

                this.sendQ = new Queue(100);

                ThreadStart ircReaderThreadStart = this.IrcReaderThreadMethod;
                this.ircReaderThread = new Thread(ircReaderThreadStart);

                ThreadStart ircWriterThreadStart = this.IrcWriterThreadMethod;
                this.ircWriterThread = new Thread(ircWriterThreadStart);

                this.RegisterInstance();
                this.ircReaderThread.Start();
                this.ircWriterThread.Start();

                this.connectionRegistrationRequiredEvent();

                return true;
            }
            catch (SocketException ex)
            {
                GlobalFunctions.errorLog(ex);
                return false;
            }
        }

        /// <summary>
        /// The send raw line.
        /// </summary>
        /// <param name="line">
        /// The line.
        /// </param>
        public void SendRawLine(string line)
        {
            this.SendLine(line);
        }

        /// <summary>
        /// The IRC pong.
        /// </summary>
        /// <param name="dataPacket">
        /// The data packet.
        /// </param>
        public void IrcPong(string dataPacket)
        {
            this.SendLine("PONG " + dataPacket);
        }

        /// <summary>
        /// The IRC ping.
        /// </summary>
        /// <param name="dataPacket">
        /// The data packet.
        /// </param>
        public void IrcPing(string dataPacket)
        {
            this.SendLine("PING " + dataPacket);
        }

        /// <summary>
        ///   Sends a private message
        /// </summary>
        /// <param name = "destination">The destination of the private message.</param>
        /// <param name = "message">The message text to be sent</param>
        public void IrcPrivmsg(string destination, string message)
        {
            if (message.Length > 400)
            {
                this.SendLine("PRIVMSG " + destination + " :" + message.Substring(0, 400) + "...");
                this.IrcPrivmsg(destination, "..." + message.Substring(400));
            }
            else
            {
                this.SendLine("PRIVMSG " + destination + " :" + message);
            }

            Linker.instance().parseMessage(message, destination);
        }

        /// <summary>
        /// The IRC quit.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void IrcQuit(string message)
        {
            this.SendLine("QUIT :" + message);
        }

        /// <summary>
        /// The IRC quit.
        /// </summary>
        public void IrcQuit()
        {
            this.SendLine("QUIT");
        }

        /// <summary>
        /// The IRC join.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void IrcJoin(string channel)
        {
            if (channel != "0")
            {
                this.SendLine("JOIN " + channel);
            }
        }

        /// <summary>
        /// The IRC join.
        /// </summary>
        /// <param name="channels">
        /// The channels.
        /// </param>
        public void IrcJoin(string[] channels)
        {
            foreach (string channel in channels)
            {
                this.IrcJoin(channel);
            }
        }

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
        public void IrcMode(string channel, string modeflags, string param)
        {
            this.SendLine("MODE " + channel + " " + modeflags + " " + param);
        }

        /// <summary>
        /// The IRC mode.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        public void IrcMode(string channel, string flags)
        {
            this.IrcMode(channel, flags, string.Empty);
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
            this.SendLine("PART " + channel + " " + message);
        }

        /// <summary>
        /// The IRC part.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void IrcPart(string channel)
        {
            this.IrcPart(channel, string.Empty);
        }

        /// <summary>
        /// The part all channels.
        /// </summary>
        public void PartAllChannels()
        {
            this.SendLine("JOIN 0");
        }

        /// <summary>
        /// The IRC names.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void IrcNames(string channel)
        {
            this.SendLine("NAMES " + channel);
        }

        /// <summary>
        /// The IRC names.
        /// </summary>
        public void IrcNames()
        {
            this.SendLine("NAMES");
        }

        /// <summary>
        /// The IRC list.
        /// </summary>
        public void IrcList()
        {
            this.SendLine("LIST");
        }

        /// <summary>
        /// The IRC list.
        /// </summary>
        /// <param name="channels">
        /// The channels.
        /// </param>
        public void IrcList(string channels)
        {
            this.SendLine("LIST " + channels);
        }

        /// <summary>
        /// The IRC invite.
        /// </summary>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void IrcInvite(string nickname, string channel)
        {
            this.SendLine("INVITE " + nickname + " " + channel);
        }

        /// <summary>
        /// The IRC kick.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        public void IrcKick(string channel, string user)
        {
            this.SendLine("KICK " + channel + " " + user);
        }

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
        public void IrcKick(string channel, string user, string reason)
        {
            this.SendLine("KICK" + channel + " " + user + " :" + reason);
        }

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
        public void CtcpReply(string destination, string command, string parameters)
        {
            ASCIIEncoding asc = new ASCIIEncoding();
            byte[] ctcp = { Convert.ToByte(1) };
            this.IrcNotice(destination, asc.GetString(ctcp) + command.ToUpper() + " " + parameters + asc.GetString(ctcp));
        }

        /* public void CtcpRequest( string destination, string command )
        {
            CtcpRequest( destination , command , string.Empty );
        }
        public void CtcpRequest( string destination , string command, string parameters )
        {
            ASCIIEncoding asc = new ASCIIEncoding( );
            byte[ ] ctcp = { Convert.ToByte( 1 ) };
            IrcPrivmsg( destination , asc.GetString( ctcp ) + command.ToUpper( ) + ( parameters == string.Empty ? "" : " " + parameters ) + asc.GetString( ctcp ) );
        }*/

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
            this.SendLine("NOTICE " + destination + " :" + message);
        }

        /// <summary>
        /// The IRC MOTD.
        /// </summary>
        /// <remarks>
        /// TODO: Expand for network staff use
        /// </remarks>
        public void IrcMotd()
        {
            this.SendLine("MOTD");
        }

        /// <summary>
        /// The IRC local users.
        /// </summary>    
        /// <remarks>
        /// TODO: Expand for network staff use
        /// </remarks>
        public void IrcLusers()
        {
            this.SendLine("LUSERS");
        }

        /// <summary>
        /// The IRC version.
        /// </summary>    
        /// <remarks>
        /// TODO: Expand for network staff use
        /// </remarks>
        public void IrcVersion()
        {
            this.SendLine("VERSION");
        }

        /// <summary>
        /// The IRC stats.
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        public void IrcStats(string query)
        {
            this.SendLine("STATS " + query);
        }

        /// <summary>
        /// The IRC links.
        /// </summary>
        /// <param name="mask">
        /// The mask.
        /// </param>
        public void IrcLinks(string mask)
        {
            this.SendLine("LINKS " + mask);
        }

        /// <summary>
        /// The IRC time.
        /// </summary>
        public void IrcTime()
        {
            this.SendLine("TIME");
        }

        /// <summary>
        /// The IRC topic.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void IrcTopic(string channel)
        {
            this.SendLine("TOPIC " + channel);
        }

        /// <summary>
        /// The IRC topic.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        public void IrcTopic(string channel, string content)
        {
            this.SendLine("TOPIC " + channel + " :" + content);
        }

        /// <summary>
        /// The IRC admin.
        /// </summary>
        public void IrcAdmin()
        {
            this.SendLine("ADMIN");
        }

        /// <summary>
        /// The IRC info.
        /// </summary>
        public void IrcInfo()
        {
            this.SendLine("INFO");
        }

        /// <summary>
        /// The IRC who.
        /// </summary>
        /// <param name="mask">
        /// The mask.
        /// </param>
        public void IrcWho(string mask)
        {
            this.SendLine("WHO " + mask);
        }

        /// <summary>
        /// The IRC whois.
        /// </summary>
        /// <param name="mask">
        /// The mask.
        /// </param>
        public void IrcWhois(string mask)
        {
            this.SendLine("WHOIS " + mask);
        }

        /// <summary>
        /// The IRC who was.
        /// </summary>
        /// <param name="mask">
        /// The mask.
        /// </param>
        public void IrcWhowas(string mask)
        {
            this.SendLine("WHOWAS " + mask);
        }

        /// <summary>
        /// The IRC kill.
        /// </summary>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="comment">
        /// The comment.
        /// </param>
        public void IrcKill(string nickname, string comment)
        {
            this.SendLine("KILL " + nickname + " :" + comment);
        }

        /// <summary>
        /// The IRC away.
        /// </summary>
        public void IrcAway()
        {
            this.SendLine("AWAY");
        }

        /// <summary>
        /// The IRC away.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void IrcAway(string message)
        {
            this.SendLine("AWAY :" + message);
        }

        /// <summary>
        /// The IRC is on.
        /// </summary>
        /// <param name="nicklist">
        /// The nick list.
        /// </param>
        public void IrcIson(string nicklist)
        {
            this.SendLine("ISON " + nicklist);
        }

        /// <summary>
        ///   Compares the channel name against the valid channel name settings returned by the IRC server on connection
        /// </summary>
        /// <param name = "channelName">Channel name to check</param>
        /// <returns>Boolean true if provided channel name is valid</returns>
        public bool IsValidChannelName(string channelName)
        {
            // TODO: make better!
            return channelName.StartsWith("#");
        }

        /// <summary>
        /// Checks if nickname is on channel
        /// </summary>
        /// <param name = "channel">channel to check</param>
        /// <param name = "nickname">nickname to check</param>
        /// <returns>1 if nickname is on channel
        /// 0 if nickname is not on channel
        /// -1 if it cannot be checked at the moment
        /// </returns>
        public int IsOnChannel(string channel, string nickname)
        {
            if (this.namesList.ContainsKey(channel))
            {
                return ((ArrayList)this.namesList[channel]).Contains(nickname) ? 1 : 0;
            }

            this.IrcNames(channel);
            return -1;
        }

        #endregion

        #region Threads

        /// <summary>
        /// The IRC reader thread method.
        /// </summary>
        private void IrcReaderThreadMethod()
        {
            bool threadIsAlive = true;
            do
            {
                try
                {
                    string line = this.ircReader.ReadLine();
                    if (line == null)
                    {
                        // noop
                    }
                    else
                    {
                        EventHandler<DataReceivedEventArgs> tempDataReceivedEventHandler = this.DataReceivedEvent;
                        if (tempDataReceivedEventHandler != null)
                        {
                            tempDataReceivedEventHandler(this, new DataReceivedEventArgs(line));
                        }
                    }
                }
                catch (ThreadAbortException ex)
                {
                    threadIsAlive = false;
                    GlobalFunctions.errorLog(ex);
                }
                catch (IOException ex)
                {
                    threadIsAlive = false;
                    GlobalFunctions.errorLog(ex);
                }
                catch (Exception ex)
                {
                    GlobalFunctions.errorLog(ex);
                }
            } 
            while (threadIsAlive);

            Console.WriteLine("*** Reader thread died.");

            EventHandler temp = this.ThreadFatalErrorEvent;
            if (temp != null)
            {
                temp(this, new EventArgs());
            }
        }

        /// <summary>
        /// The IRC writer thread method.
        /// </summary>
        private void IrcWriterThreadMethod()
        {
            bool threadIsAlive = true;
            do
            {
                try
                {
                    string line = null;
                    lock (this.sendQ)
                    {
                        if (this.sendQ.Count > 0)
                        {
                            line = (string)this.sendQ.Dequeue();
                        }
                    }

                    if (line != null)
                    {
                        Logger.instance().addToLog("< " + line, Logger.LogTypes.IAL);
                        this.ircWriter.WriteLine(line);
                        this.ircWriter.Flush();
                        Thread.Sleep(this.FloodProtectionWaitTime);
                    }
                    else
                    {
                        // wait a short while before rechecking
                        Thread.Sleep(100);
                    }
                }
                catch (ThreadAbortException ex)
                {
                    threadIsAlive = false;
                    GlobalFunctions.errorLog(ex);
                    this.sendQ.Clear();
                }
                catch (IOException ex)
                {
                    threadIsAlive = false;
                    GlobalFunctions.errorLog(ex);
                }
                catch (Exception ex)
                {
                    GlobalFunctions.errorLog(ex);
                }
            }
            while (threadIsAlive && this.ircReaderThread.IsAlive);

            Console.WriteLine("*** Writer thread died.");

            EventHandler temp = this.ThreadFatalErrorEvent;
            if (temp != null)
            {
                temp(this, new EventArgs());
            }
        }

        #endregion

        /// <summary>
        /// The initialise event handlers.
        /// </summary>
        private void InitialiseEventHandlers()
        {
            this.DataReceivedEvent += this.IrcDataReceivedEvent;
            this.UnrecognisedDataReceivedEvent += this.UnrecognisedDataReceivedEventHandler;
            this.connectionRegistrationRequiredEvent += this.RegisterConnection;
            this.pingEvent += this.IrcPong;
            this.nicknameChangeEvent += this.IrcNicknameChangeEvent;
            this.quitEvent += this.IrcQuitEvent;
            this.joinEvent += this.IrcJoinEvent;
            this.partEvent += this.IrcPartEvent;
            this.topicEvent += this.IrcTopicEvent;
            this.modeChangeEvent += this.IrcModeChangeEvent;
            this.inviteEvent += this.IrcInviteEvent;
            this.kickEvent += this.IrcKickEvent;
            this.PrivateMessageEvent += this.IrcPrivateMessageEvent;
            this.ClientToClientEvent += this.IrcClientToClientEvent;
            this.NoticeEvent += this.IrcNoticeEvent;
            this.errNicknameInUseEvent += this.AssumeTakenNickname;
            this.errUnavailResource += this.IrcErrorUnavailResource;
            this.nameReplyEvent += this.IrcNameReplyEvent;
            this.connectionRegistrationSucceededEvent += this.IrcConnectionRegistrationSucceededEvent;
        }

        /// <summary>
        /// The unrecognised data received event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void UnrecognisedDataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            this.Log("DATA RECIEVED EVENT WITH DATA " + e.Data);
        }

        /// <summary>
        /// The IRC connection registration succeeded event.
        /// </summary>
        private void IrcConnectionRegistrationSucceededEvent()
        {
            this.IrcPrivmsg(this.nickserv, "IDENTIFY " + this.myNickname + " " + this.myPassword);
            this.IrcMode(this.myNickname, "+Q");
        }

        /// <summary>
        /// The IRC error unavailable resource.
        /// </summary>
        private void IrcErrorUnavailResource()
        {
            if (this.nickserv != string.Empty)
            {
                this.AssumeTakenNickname();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #region event handlers

        /// <summary>
        /// The my info reply event.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        private void ReplyMyInfoEvent(string parameters)
        {
            this.rplMyinfo = parameters;
        }

        /// <summary>
        /// The created reply event.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        private void ReplyCreatedEvent(string parameters)
        {
        }

        /// <summary>
        /// The reply your host event.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        private void ReplyYourHostEvent(string parameters)
        {
        }

        /// <summary>
        /// The IRC name reply event.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="names">
        /// The names.
        /// </param>
        private void IrcNameReplyEvent(string channel, IEnumerable<string> names)
        {
            if (!this.namesList.ContainsKey(channel))
            {
                return;
            }

            foreach (string name in names)
            {
                ArrayList channelNamesList = (ArrayList)this.namesList[channel];
                string newName = name.Trim('@', '+');
                if (!channelNamesList.Contains(newName))
                {
                    channelNamesList.Add(newName);
                }
            }
        }

        /// <summary>
        /// The IRC notice event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void IrcNoticeEvent(object sender, PrivateMessageEventArgs e)
        {
            this.Log("NOTICE EVENT FROM " + e.Sender + " TO " + e.Destination + " MESSAGE " + e.Message);
        }

        /// <summary>
        /// The IRC CTCP event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void IrcClientToClientEvent(object sender, PrivateMessageEventArgs e)
        {
            this.Log("CTCP EVENT FROM " + e.Sender + " TO " + e.Destination + " MESSAGE " + e.Message);
            switch (e.Message.Split(' ')[0].ToUpper())
            {
                case "VERSION":
                    this.CtcpReply(e.Sender.nickname, "VERSION", this.ClientVersion);
                    break;
                case "TIME":
                    this.CtcpReply(e.Sender.nickname, "TIME", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    break;
                case "PING":
                    this.CtcpReply(e.Sender.nickname, "PING", e.Message.Split(' ')[1]);
                    break;
                case "FINGER":
                    this.CtcpReply(e.Sender.nickname, "FINGER", this.RealName + ", idle " + this.IdleTime);
                    break;
            }
        }

        /// <summary>
        /// The IRC private (channel) message event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void IrcPrivateMessageEvent(object sender, PrivateMessageEventArgs e)
        {
            // Don't re-enable.
            // this.log("PRIVMSG EVENT FROM " + source + " TO " + destination + " MESSAGE " + message);
        }

        /// <summary>
        /// The IRC kick event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="nick">
        /// The nick.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void IrcKickEvent(User source, string channel, string nick, string message)
        {
            this.Log("KICK FROM " + channel + " BY " + source + " AFFECTED " + nick + " REASON " + message);
        }

        /// <summary>
        /// The IRC invite event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private void IrcInviteEvent(User source, string nickname, string channel)
        {
            this.Log("INVITE FROM " + source + " TO " + nickname + " CHANNEL " + channel);
        }

        /// <summary>
        /// The IRC mode change event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="flagChanges">
        /// The flag changes.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private void IrcModeChangeEvent(User source, string subject, string flagChanges, string parameter)
        {
            this.Log("MODE CHANGE BY " + source + " ON " + subject + " CHANGES " + flagChanges + " PARAMETER " + parameter);
        }

        /// <summary>
        /// The IRC topic event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="topic">
        /// The topic.
        /// </param>
        private void IrcTopicEvent(User source, string channel, string topic)
        {
            this.Log("TOPIC CHANGED BY " + source + " IN " + channel + " TOPIC " + topic);
        }

        /// <summary>
        /// The IRC part event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void IrcPartEvent(User source, string channel, string message)
        {
            this.Log("PART BY " + source + " FROM " + channel + " MESSAGE " + message);
            if (source.nickname == this.Nickname)
            {
                this.channelList.Remove(channel);
            }
        }

        /// <summary>
        /// The IRC join event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private void IrcJoinEvent(User source, string channel)
        {
            this.Log("JOIN EVENT BY " + source + " INTO " + channel);
            if (source.nickname == this.Nickname)
            {
                this.channelList.Add(channel);
            }
        }

        /// <summary>
        /// The IRC quit event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void IrcQuitEvent(User source, string message)
        {
            this.Log("QUIT BY " + source + " MESSAGE " + message);
        }

        /// <summary>
        /// The IRC nickname change event.
        /// </summary>
        /// <param name="oldNick">
        /// The old nick.
        /// </param>
        /// <param name="newNick">
        /// The new nick.
        /// </param>
        private void IrcNicknameChangeEvent(string oldNick, string newNick)
        {
            this.Log("NICK CHANGE BY " + oldNick + " TO " + newNick);
        }

        #endregion

        /// <summary>
        /// The IRC data received event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void IrcDataReceivedEvent(object sender, DataReceivedEventArgs e)
        {
            Logger.instance().addToLog(e.Data, Logger.LogTypes.IRC);

            char[] colonSeparator = { ':' };

            string command, parameters;
            string messagesource = command = parameters = string.Empty;
            BasicParser(e.Data, ref messagesource, ref command, ref parameters);

            User source = new User();

            if (messagesource != null)
            {
                source = User.newFromString(messagesource, this.networkId);
            }

            switch (command)
            {
                case "ERROR":
                    if (parameters.ToLower().Contains(":closing link"))
                    {
                        this.tcpClient.Close();
                        this.ircReaderThread.Abort();
                        this.ircWriterThread.Abort();
                    }

                    break;
                case "PING":
                    this.pingEvent(parameters);
                    break;
                case "NICK":
                    this.nicknameChangeEvent(source.nickname, parameters.Substring(1));
                    break;
                case "MODE":
                    try
                    {
                        string subject = parameters.Split(' ')[0];
                        string flagchanges = parameters.Split(' ')[1];
                        string param = parameters.Split(' ').Length > 2 ? parameters.Split(' ')[2] : string.Empty;

                        this.modeChangeEvent(source, subject, flagchanges, param);
                    }
                    catch (NullReferenceException ex)
                    {
                        GlobalFunctions.errorLog(ex);
                    }

                    break;
                case "QUIT":
                    this.quitEvent(source, parameters);
                    break;
                case "JOIN":
                    this.joinEvent(source, parameters);
                    break;
                case "PART":
                    string s = parameters.Contains(new string(colonSeparator))
                                   ? parameters.Split(colonSeparator, 2)[1]
                                   : string.Empty;
                    this.partEvent(source, parameters.Split(' ')[0], s);
                    break;
                case "TOPIC":
                    this.topicEvent(source, parameters.Split(' ')[0], parameters.Split(colonSeparator, 2)[1]);
                    break;
                case "INVITE":
                    this.inviteEvent(source, parameters.Split(' ')[0], parameters.Split(' ')[1].Substring(1));
                    break;
                case "KICK":
                    this.kickEvent(
                        source,
                        parameters.Split(' ')[0],
                        parameters.Split(' ')[1],
                        parameters.Split(colonSeparator, 2)[1]);
                    break;
                case "PRIVMSG":
                    string message = parameters.Split(colonSeparator, 2)[1];
                    ASCIIEncoding asc = new ASCIIEncoding();
                    byte[] ctcp = { Convert.ToByte(1) };

                    string destination = parameters.Split(colonSeparator, 2)[0].Trim();
                    if (destination == this.Nickname)
                    {
                        destination = source.nickname;
                    }

                    if (message.StartsWith(asc.GetString(ctcp)))
                    {
                        this.ClientToClientEvent(
                            this,
                            new PrivateMessageEventArgs(
                                source,
                                destination,
                                message.Trim(Convert.ToChar(Convert.ToByte(1)))));
                    }
                    else
                    {
                        this.PrivateMessageEvent(this, new PrivateMessageEventArgs(source, destination, message.Trim()));
                    }
                    
                    break;

                case "NOTICE":
                    string noticedestination = parameters.Split(colonSeparator, 2)[0].Trim();
                    if (noticedestination == this.Nickname)
                    {
                        noticedestination = source.nickname;
                    }

                    this.NoticeEvent(
                        this,
                        new PrivateMessageEventArgs(source, noticedestination, parameters.Split(colonSeparator, 2)[1]));
                    break;
                case "001":
                    this.connectionRegistrationSucceededEvent();
                    break;
                case "002":
                    this.ReplyYourHostEvent(parameters);
                    break;
                case "003":
                    this.ReplyCreatedEvent(parameters);
                    break;
                case "004":
                    this.ReplyMyInfoEvent(parameters);
                    break;
                case "433":
                    this.errNicknameInUseEvent();
                    break;
                case "437":
                    this.errUnavailResource();
                    break;
                default:
                    var temp = this.UnrecognisedDataReceivedEvent;
                    if (temp != null)
                    {
                        temp(this, e);
                    }

                    break;
            }
        }

        #region parsers

        /// <summary>
        /// The basic parser.
        /// </summary>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <remarks>
        /// TODO: Refactor me to return an object
        /// </remarks>
        public static void BasicParser(string line, ref string source, ref string command, ref string parameters)
        {
            char[] stringSplitter = { ' ' };
            string[] parseBasic;
            if (line.Substring(0, 1) == ":")
            {
                parseBasic = line.Split(stringSplitter, 3);
                source = parseBasic[0].Substring(1);
                command = parseBasic[1];
                parameters = parseBasic[2];
            }
            else
            {
                parseBasic = line.Split(stringSplitter, 2);
                source = null;
                command = parseBasic[0];
                parameters = parseBasic[1];
            }
        }

        #endregion

        /// <summary>
        /// The log.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void Log(string message)
        {
            if (this.LogEvents)
            {
                Logger.instance().addToLog("<" + this.networkId + ">" + message, Logger.LogTypes.IAL);
            }
        }

        #region IThreadedSystem Members

        /// <summary>
        /// The Stop.
        /// </summary>
        public void Stop()
        {
            this.IrcQuit("Requested by controller");
            Thread.Sleep(5000);
            this.ircWriterThread.Abort();
            this.ircReaderThread.Abort();
        }

        /// <summary>
        /// The register instance.
        /// </summary>
        public void RegisterInstance()
        {
            ThreadList.instance().register(this);
        }

        /// <summary>
        /// The get thread status.
        /// </summary>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public string[] GetThreadStatus()
        {
            string[] statuses =
                {
                    "(" + this.networkId + ") " + this.Server + " READER:"
                    + this.ircReaderThread.ThreadState,
                    "(" + this.networkId + ") " + this.Server + " WRITER:"
                    + this.ircWriterThread.ThreadState
                };
            return statuses;
        }

        /// <summary>
        /// The thread fatal error event.
        /// </summary>
        public event EventHandler ThreadFatalErrorEvent;

        #endregion

        #region Private Methods
        /// <summary>
        /// The send line.
        /// </summary>
        /// <param name="line">
        /// The line.
        /// </param>
        private void SendLine(string line)
        {
            if (!this.IsConnected)
            {
                return;
            }

            line = line.Replace("\n", " ");
            line = line.Replace("\r", " ");
            lock (this.sendQ)
            {
                this.sendQ.Enqueue(line.Trim());
            }

            this.MessageCount++;
        }

        /// <summary>
        /// The send nick.
        /// </summary>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        private void SendNick(string nickname)
        {
            this.SendLine("NICK " + nickname);
        }

        /// <summary>
        /// The register connection.
        /// </summary>
        private void RegisterConnection()
        {
            if (this.myPassword != null)
            {
                this.SendLine("PASS " + this.myPassword);
            }

            this.SendLine("USER " + this.myUsername + " " + "*" + " * :" + this.myRealname);
            this.SendNick(this.myNickname);
        }

        /// <summary>
        /// The assume taken nickname.
        /// </summary>
        private void AssumeTakenNickname()
        {
            this.SendNick(this.myNickname + "_");
            if (this.nickserv == string.Empty)
            {
                return;
            }

            this.IrcPrivmsg(this.nickserv, "GHOST " + this.myNickname + " " + this.myPassword);
            this.IrcPrivmsg(this.nickserv, "RELEASE " + this.myNickname + " " + this.myPassword);
            this.SendNick(this.myNickname);
        }
        #endregion
    }
}