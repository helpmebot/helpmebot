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
    using System.Diagnostics.CodeAnalysis;
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
        #region internal variables

        private Thread _ircReaderThread;
        private Thread _ircWriterThread;

        private readonly string _myNickname;
        private readonly string _myUsername;
        private readonly string _myRealname;
        private readonly string _myPassword;

        private readonly string _nickserv;

        private TcpClient _tcpClient;
        private StreamReader _ircReader;
        private StreamWriter _ircWriter;

        private Queue _sendQ;

        private readonly Hashtable _namesList = new Hashtable();

        private readonly DateTime _lastMessage = DateTime.Now;

        private readonly uint _networkId;

        private ArrayList channelList = new ArrayList();

        private string _rpl_myinfo;
        #endregion

        #region constructor/destructor

        public IrcAccessLayer(uint ircNetwork)
        {
            this.FloodProtectionWaitTime = 500;
            this.ClientVersion = "Helpmebot IRC Access Layer 1.0";
            this._networkId = ircNetwork;

            DAL db = DAL.singleton();


            DAL.Select q = new DAL.Select("in_host", "in_port", "in_nickname", "in_password", "in_username",
                                          "in_realname", "in_log", "in_nickserv");
            q.setFrom("ircnetwork");
            q.addLimit(1, 0);
            q.addWhere(new DAL.WhereConds("in_id", ircNetwork.ToString()));

            ArrayList configSettings = db.executeSelect(q);

            this.Server = (string)(((object[])configSettings[0])[0]);
            this.Port = (uint)((object[])configSettings[0])[1];

            this._myNickname = (string)(((object[])configSettings[0])[2]);
            this._myPassword = (string)(((object[])configSettings[0])[3]);
            this._myUsername = (string)(((object[])configSettings[0])[4]);
            this._myRealname = (string)(((object[])configSettings[0])[5]);

            this.LogEvents = (bool)(((object[])configSettings[0])[6]);

            this._nickserv = (string)(((object[])configSettings[0])[7]);

            if ( /*recieveWallops*/ true)
                this.ConnectionUserModes += 4;
            if ( /*invisible*/ true)
                this.ConnectionUserModes += 8;

            this.initialiseEventHandlers();
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
            this._networkId = 0;
            this.Server = server;
            this.Port = port;

            this._myNickname = nickname;
            this._myPassword = password;
            this._myUsername = username;
            this._myRealname = realname;

            this.initialiseEventHandlers();
        }

        /// <summary>
        /// Finalises an instance of the <see cref="IrcAccessLayer"/> class. 
        /// </summary>
        ~IrcAccessLayer()
        {
            if (this._tcpClient.Connected)
            {
                this.ircQuit();
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

        private event ConnectionRegistrationEventHandler connectionRegistrationRequiredEvent;
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

        public event PrivmsgEventHandler privmsgEvent;
        public event PrivmsgEventHandler ctcpEvent;
        public event PrivmsgEventHandler noticeEvent;

        public delegate void IrcEventHandler();

        public event IrcEventHandler errNicknameInUseEvent;
        public event IrcEventHandler errUnavailResource;

        public delegate void NameReplyEventHandler(string channel, string[] names);

        // TODO: invoke this event somewhere
        public event NameReplyEventHandler nameReplyEvent;


        #endregion

        #region properties

        /// <summary>
        /// Gets the server info.
        /// </summary>
        public string ServerInfo
        {
            get
            {
                return this._rpl_myinfo;
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
                return this._tcpClient != null && this._tcpClient.Connected;
            }
        }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public string Nickname
        {
            get
            {
                return this._myNickname;
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
            get { return this._myUsername; }
        }

        /// <summary>
        /// Gets the real name.
        /// </summary>
        public string RealName
        {
            get { return this._myRealname; }
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
            get { return DateTime.Now.Subtract(this._lastMessage); }
        }

        /// <summary>
        /// Gets a value indicating whether log events.
        /// </summary>
        public bool LogEvents { get; private set; }

        public string[] activeChannels
        {
            get
            {
                Type t = Type.GetType("System.String");
                return (string[])this.channelList.ToArray(t);
            }
        }
        
        #endregion

        #region Methods

        public bool connect()
        {
            try
            {
                this._tcpClient = new TcpClient(this.Server, (int) this.Port);

                Stream ircStream = this._tcpClient.GetStream();
                this._ircReader = new StreamReader(ircStream, Encoding.UTF8);
                this._ircWriter = new StreamWriter(ircStream, Encoding.UTF8);


                this._sendQ = new Queue(100);

                ThreadStart ircReaderThreadStart = this.IrcReaderThreadMethod;
                this._ircReaderThread = new Thread(ircReaderThreadStart);

                ThreadStart ircWriterThreadStart = this.IrcWriterThreadMethod;
                this._ircWriterThread = new Thread(ircWriterThreadStart);

                this.registerInstance();
                this._ircReaderThread.Start();
                this._ircWriterThread.Start();

                this.connectionRegistrationRequiredEvent();

                return true;
            }
            catch (SocketException ex)
            {
                GlobalFunctions.errorLog(ex);
                return false;
            }
        }

        private void _sendLine(string line)
        {
            if ( !this.IsConnected ) return;
            line = line.Replace("\n", " ");
            line = line.Replace("\r", " ");
            lock (this._sendQ)
            {
                this._sendQ.Enqueue(line.Trim());
            }
            this.MessageCount++;
        }

        private void _sendPass(string password)
        {
            this._sendLine("PASS " + password);
        }

        private void _sendNick(string nickname)
        {
            this._sendLine("NICK " + nickname);
        }

        /// <summary>
        ///   Sends the USER command as part of connection registration
        /// </summary>
        /// <param name = "username">The client's username</param>
        /// <param name = "realname">The client's real name</param>
        private void _sendUser(string username, string realname)
        {
            this._sendLine("USER " + username + " " + "*" + " * :" + realname);
        }

        private void registerConnection()
        {
            if (this._myPassword != null)
                this._sendPass(this._myPassword);
            this._sendUser(this._myUsername, this._myRealname);
            this._sendNick(this._myNickname);
        }

        private void assumeTakenNickname()
        {
            this._sendNick(this._myNickname + "_");
            if ( this._nickserv == string.Empty ) return;
            this.ircPrivmsg(this._nickserv, "GHOST " + this._myNickname + " " + this._myPassword);
            this.ircPrivmsg(this._nickserv, "RELEASE " + this._myNickname + " " + this._myPassword);
            this._sendNick(this._myNickname);
        }

        public void sendRawLine(string line)
        {
            this._sendLine(line);
        }

        public void ircPong(string datapacket)
        {
            this._sendLine("PONG " + datapacket);
        }

        public void ircPing(string datapacket)
        {
            this._sendLine("PING " + datapacket);
        }

        /// <summary>
        ///   Sends a private message
        /// </summary>
        /// <param name = "destination">The destination of the private message.</param>
        /// <param name = "message">The message text to be sent</param>
        public void ircPrivmsg(string destination, string message)
        {
            if (message.Length > 400)
            {
                this._sendLine("PRIVMSG " + destination + " :" + message.Substring(0, 400) + "...");
                this.ircPrivmsg(destination, "..." + message.Substring(400));
            }
            else
            {
                this._sendLine("PRIVMSG " + destination + " :" + message);
            }
            Linker.instance().parseMessage(message, destination);
        }

        public void ircQuit(string message)
        {
            this._sendLine("QUIT :" + message);
        }

        public void ircQuit()
        {
            this._sendLine("QUIT");
        }

        public void ircJoin(string channel)
        {
            this._sendLine("JOIN " + channel);
        }

        public void ircJoin(string[] channels)
        {
            foreach (string channel in channels)
            {
                this.ircJoin(channel);
            }
        }

        public void ircMode(string channel, string modeflags, string param)
        {
            this._sendLine("MODE " + channel + " " + modeflags + " " + param);
        }

        public void ircMode(string channel, string flags)
        {
            this.ircMode(channel, flags, "");
        }

        public void ircPart(string channel, string message)
        {
            this._sendLine("PART " + channel + " " + message);
        }

        public void ircPart(string channel)
        {
            this.ircPart(channel, "");
        }

        public void partAllChannels()
        {
            this.ircJoin("0");
        }

        public void ircNames(string channel)
        {
            this._sendLine("NAMES " + channel);
        }

        public void ircNames()
        {
            this._sendLine("NAMES");
        }

        public void ircList()
        {
            this._sendLine("LIST");
        }

        public void ircList(string channels)
        {
            this._sendLine("LIST " + channels);
        }

        public void ircInvite(string nickname, string channel)
        {
            this._sendLine("INVITE " + nickname + " " + channel);
        }

        public void ircKick(string channel, string user)
        {
            this._sendLine("KICK " + channel + " " + user);
        }

        public void ircKick(string channel, string user, string reason)
        {
            this._sendLine("KICK" + channel + " " + user + " :" + reason);
        }

        public void ctcpReply(string destination, string command, string parameters)
        {
            ASCIIEncoding asc = new ASCIIEncoding();
            byte[] ctcp = {Convert.ToByte(1)};
            this.ircNotice(destination, asc.GetString(ctcp) + command.ToUpper() + " " + parameters + asc.GetString(ctcp));
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

        public static string wrapCTCP(string command, string parameters)
        {
            ASCIIEncoding asc = new ASCIIEncoding();
            byte[] ctcp = {Convert.ToByte(1)};
            return (asc.GetString(ctcp) + command.ToUpper() + (parameters == string.Empty ? "" : " " + parameters) +
                    asc.GetString(ctcp));
        }

        public void ircNotice(string destination, string message)
        {
            this._sendLine("NOTICE " + destination + " :" + message);
        }

        //TODO: Expand for network staff use
        public void ircMotd()
        {
            this._sendLine("MOTD");
        }

        //TODO: Expand for network staff use
        public void ircLusers()
        {
            this._sendLine("LUSERS");
        }

        //TODO: Expand for network staff use
        public void ircVersion()
        {
            this._sendLine("VERSION");
        }

        public void ircStats(string query)
        {
            this._sendLine("STATS " + query);
        }

        public void ircLinks(string mask)
        {
            this._sendLine("LINKS " + mask);
        }

        public void ircTime()
        {
            this._sendLine("TIME");
        }

        public void ircTopic(string channel)
        {
            this._sendLine("TOPIC " + channel);
        }

        public void ircTopic(string channel,string content)
        {
            this._sendLine("TOPIC " + channel + " :" + content);
        }

        public void ircAdmin()
        {
            this._sendLine("ADMIN");
        }

        public void ircInfo()
        {
            this._sendLine("INFO");
        }

        public void ircWho(string mask)
        {
            this._sendLine("WHO " + mask);
        }

        public void ircWhois(string mask)
        {
            this._sendLine("WHOIS " + mask);
        }

        public void ircWhowas(string mask)
        {
            this._sendLine("WHOWAS " + mask);
        }

        public void ircKill(string nickname, string comment)
        {
            this._sendLine("KILL " + nickname + " :" + comment);
        }

        public void ircAway()
        {
            this._sendLine("AWAY");
        }

        public void ircAway(string message)
        {
            this._sendLine("AWAY :" + message);
        }

        public void ircIson(string nicklist)
        {
            this._sendLine("ISON " + nicklist);
        }

        /// <summary>
        ///   Compares the channel name against the valid channel name settings returned by the IRC server on connection
        /// </summary>
        /// <param name = "channelName">Channel name to check</param>
        /// <returns>Boolean true if provided channel name is valid</returns>
        public bool isValidChannelName(string channelName)
        {
            // TODO: make better!
            return channelName.StartsWith("#");
        }

        /// <summary>
        ///   checks if nickname is on channel
        /// </summary>
        /// <param name = "channel">channel to check</param>
        /// <param name = "nickname">nickname to check</param>
        /// <returns>1 if nickname is on channel
        ///   0 if nickname is not on channel
        ///   -1 if it cannot be checked at the moment</returns>
        public int isOnChannel(string channel, string nickname)
        {
            if (this._namesList.ContainsKey(channel))
            {
                return ((ArrayList) this._namesList[channel]).Contains(nickname) ? 1 : 0;
            }
            this.ircNames(channel);
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
                    string line = this._ircReader.ReadLine();
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

            EventHandler temp = this.threadFatalError;
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
                    lock (this._sendQ)
                    {
                        if (this._sendQ.Count > 0)
                            line = (string) this._sendQ.Dequeue();
                    }

                    if (line != null)
                    {
                        Logger.instance().addToLog("< " + line, Logger.LogTypes.IAL);
                        this._ircWriter.WriteLine(line);
                        this._ircWriter.Flush();
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
                    this._sendQ.Clear();
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
            } while (threadIsAlive && this._ircReaderThread.IsAlive);

            Console.WriteLine("*** Writer thread died.");

            EventHandler temp = this.threadFatalError;
            if (temp != null)
            {
                temp(this, new EventArgs());
            }
        }

        #endregion

        private void initialiseEventHandlers()
        {
            this.DataReceivedEvent += this.ialDataRecievedEvent;
            this.UnrecognisedDataReceivedEvent += this.UnrecognisedDataReceivedEventHandler;
            this.connectionRegistrationRequiredEvent += this.registerConnection;
            this.pingEvent += this.ircPong;
            this.nicknameChangeEvent += this.ialNicknameChangeEvent;
            this.quitEvent += this.ialQuitEvent;
            this.joinEvent += this.ialJoinEvent;
            this.partEvent += this.ialPartEvent;
            this.topicEvent += this.ialTopicEvent;
            this.modeChangeEvent += this.ialModeChangeEvent;
            this.inviteEvent += this.ialInviteEvent;
            this.kickEvent += this.ialKickEvent;
            this.privmsgEvent += this.ialPrivmsgEvent;
            this.ctcpEvent += this.ialCtcpEvent;
            this.noticeEvent += this.ialNoticeEvent;
            this.errNicknameInUseEvent += this.assumeTakenNickname;
            this.errUnavailResource += this.ialErrUnavailResource;
            this.nameReplyEvent += this.ialNameReplyEvent;
            this.connectionRegistrationSucceededEvent += this.ialConnectionRegistrationSucceededEvent;
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
            this.log("DATA RECIEVED EVENT WITH DATA " + e.Data);
        }

        void ialConnectionRegistrationSucceededEvent()
        {
            this.ircPrivmsg(this._nickserv, "IDENTIFY " + this._myNickname + " " + this._myPassword);
            this.ircMode(this._myNickname, "+Q");
        }

        private void ialErrUnavailResource()
        {
            if (this._nickserv != string.Empty)
            {
                this.assumeTakenNickname();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #region event handlers

        private void RPL_MyInfoEvent(string parameters)
        {
            this._rpl_myinfo = parameters;
        }

        private void RPL_CreatedEvent(string parameters)
        {

        }

        private void RPL_YourHostEvent(string parameters)
        {

        }

        private void ialNameReplyEvent(string channel, IEnumerable<string> names)
        {
            if ( !this._namesList.ContainsKey( channel ) ) return;
            foreach (string name in names)
            {
                ArrayList channelNamesList = (ArrayList) this._namesList[channel];
                string newName = name.Trim('@', '+');
                if (!channelNamesList.Contains(newName))
                    channelNamesList.Add(newName);
            }
        }

        private void ialNoticeEvent(User source, string destination, string message)
        {
            this.log("NOTICE EVENT FROM " + source + " TO " + destination + " MESSAGE " + message);
        }

        private void ialCtcpEvent(User source, string destination, string message)
        {
            this.log("CTCP EVENT FROM " + source + " TO " + destination + " MESSAGE " + message);
            switch (message.Split(' ')[0].ToUpper())
            {
                case "VERSION":
                    this.ctcpReply(source.nickname, "VERSION", this.ClientVersion);
                    break;
                case "TIME":
                    this.ctcpReply(source.nickname, "TIME", DateTime.Now.ToString());
                    break;
                case "PING":
                    this.ctcpReply(source.nickname, "PING", message.Split(' ')[1]);
                    break;
                case "FINGER":
                    this.ctcpReply(source.nickname, "FINGER", this.RealName + ", idle " + this.IdleTime);
                    break;
                default:
                    break;
            }
        }

        private void ialPrivmsgEvent(User source, string destination, string message)
        {
            // Don't re-enable.
            // this.log("PRIVMSG EVENT FROM " + source + " TO " + destination + " MESSAGE " + message);
        }

        private void ialKickEvent(User source, string channel, string nick, string message)
        {
            this.log("KICK FROM " + channel + " BY " + source + " AFFECTED " + nick + " REASON " + message);
        }

        private void ialInviteEvent(User source, string nickname, string channel)
        {
            this.log("INVITE FROM " + source + " TO " + nickname + " CHANNEL " + channel);
        }

        private void ialModeChangeEvent(User source, string subject, string flagchanges, string parameter)
        {
            this.log("MODE CHANGE BY " + source + " ON " + subject + " CHANGES " + flagchanges + " PARAMETER " + parameter);
        }

        private void ialTopicEvent(User source, string channel, string topic)
        {
            this.log("TOPIC CHANGED BY " + source + " IN " + channel + " TOPIC " + topic);
        }

        private void ialPartEvent(User source, string channel, string message)
        {
            this.log("PART BY " + source + " FROM " + channel + " MESSAGE " + message);
            if (source.nickname == this.Nickname) this.channelList.Remove(channel);
        }

        private void ialJoinEvent(User source, string channel)
        {
            this.log("JOIN EVENT BY " + source + " INTO " + channel);
            if(source.nickname == this.Nickname) this.channelList.Add(channel);
        }

        private void ialQuitEvent(User source, string message)
        {
            this.log("QUIT BY " + source + " MESSAGE " + message);
        }

        private void ialNicknameChangeEvent(string oldnick, string newnick)
        {
            this.log("NICK CHANGE BY " + oldnick + " TO " + newnick);
        }

        #endregion

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
        private void ialDataRecievedEvent(object sender, DataReceivedEventArgs e)
        {
            Logger.instance().addToLog(e.Data, Logger.LogTypes.IRC);

            char[] colonSeparator = {':'};

            string command, parameters;
            string messagesource = command = parameters = string.Empty;
            basicParser(e.Data, ref messagesource, ref command, ref parameters);

            User source = new User();

            if (messagesource != null)
            {
                source = User.newFromString(messagesource, this._networkId);
            }

            switch (command)
            {
                case "ERROR":
                    if (parameters.ToLower().Contains(":closing link"))
                    {
                        this._tcpClient.Close();
                        this._ircReaderThread.Abort();
                        this._ircWriterThread.Abort();
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
                        string param = parameters.Split(' ').Length > 2 ? parameters.Split(' ')[2] : "";

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
                    string s = parameters.Contains(new String(colonSeparator))
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
                        this.ctcpEvent(source, destination, message.Trim(Convert.ToChar(Convert.ToByte(1))));
                    }
                    else
                    {
                        this.privmsgEvent(source, destination, message.Trim());
                    }
                    
                    break;

                case "NOTICE":
                    string noticedestination = parameters.Split(colonSeparator, 2)[0].Trim();
                    if (noticedestination == this.Nickname)
                    {
                        noticedestination = source.nickname;
                    }
                    
                    this.noticeEvent(source, noticedestination, parameters.Split(colonSeparator, 2)[1]);
                    break;
                case "001":
                    this.connectionRegistrationSucceededEvent();
                    break;
                case "002":
                    this.RPL_YourHostEvent(parameters);
                    break;
                case "003":
                    this.RPL_CreatedEvent(parameters);
                    break;
                case "004":
                    this.RPL_MyInfoEvent(parameters);
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

        public static void basicParser(string line, ref string source, ref string command, ref string parameters)
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

        private void log(string message)
        {
            if (this.LogEvents)
            {
                Logger.instance().addToLog("<" + this._networkId + ">" + message, Logger.LogTypes.IAL);
            }
        }

        #region IThreadedSystem Members

        /// <summary>
        /// The stop.
        /// </summary>
        public void stop()
        {
            this.ircQuit("Requested by controller");
            Thread.Sleep(5000);
            this._ircWriterThread.Abort();
            this._ircReaderThread.Abort();
        }

        /// <summary>
        /// The register instance.
        /// </summary>
        public void registerInstance()
        {
            ThreadList.instance().register(this);
        }

        /// <summary>
        /// The get thread status.
        /// </summary>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public string[] getThreadStatus()
        {
            string[] statuses =
                {
                    "(" + this._networkId + ") " + this.Server + " READER:"
                    + this._ircReaderThread.ThreadState,
                    "(" + this._networkId + ") " + this.Server + " WRITER:"
                    + this._ircWriterThread.ThreadState
                };
            return statuses;
        }

        public event EventHandler threadFatalError;

        #endregion
    }
}