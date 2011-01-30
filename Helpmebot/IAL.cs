// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using helpmebot6.Threading;

#endregion

namespace helpmebot6
{
    using System.Collections.Generic;

    /// <summary>
    ///   IRC Access Layer
    /// 
    ///   Provides an interface to IRC.
    /// </summary>
    public class IAL : IThreadedSystem
    {
        #region internal variables

        private readonly string _myNickname;
        private readonly string _myUsername;
        private readonly string _myRealname;
        private readonly string _myPassword;

        private readonly string _ircServer;
        private readonly uint _ircPort;

        private readonly string _nickserv;

        private TcpClient _tcpClient;
        private StreamReader _ircReader;
        private StreamWriter _ircWriter;

        private Queue _sendQ;

        private readonly Hashtable _namesList = new Hashtable();

        private readonly DateTime _lastMessage = DateTime.Now;

        private readonly uint _networkId;

        private ArrayList channelList = new ArrayList();
        #endregion

        #region properties
        public string clientVersion { get; set; }

        public bool connected
        {
            get { return this._tcpClient != null && this._tcpClient.Connected; }
        }

        public string ircNickname
        {
            get { return _myNickname; }
            set { throw new NotImplementedException();}
        }

        public string ircUsername
        {
            get { return _myUsername; }
        }

        public string ircRealname
        {
            get { return _myRealname; }
        }

        public string ircServer
        {
            get { return _ircServer; }
        }

        public uint ircPort
        {
            get { return _ircPort; }
        }

        public string myIdentity
        {
            get { return ircNickname + "!" + ircUsername + "@wikimedia/bot/helpmebot"; }
        }

        public int floodProtectionWaitTime { get; set; }

        /// <summary>
        ///   +4 if recieving wallops, +8 if invisible
        /// </summary>
        public int connectionUserModes { get; private set; }

        public int messageCount { get; private set; }

        public TimeSpan idleTime
        {
            get { return DateTime.Now.Subtract(this._lastMessage); }
        }

        public bool logEvents { get; private set; }

        public string[] activeChannels { get
        {
            return (string[])channelList.ToArray(Type.GetType("String"));
        } }
        #endregion

        #region constructor/destructor

        public IAL(uint ircNetwork)
        {
            this.floodProtectionWaitTime = 500;
            this.clientVersion = "Helpmebot IRC Access Layer 1.0";
            _networkId = ircNetwork;

            DAL db = DAL.singleton();


            DAL.Select q = new DAL.Select("in_host", "in_port", "in_nickname", "in_password", "in_username",
                                          "in_realname", "in_log", "in_nickserv");
            q.setFrom("ircnetwork");
            q.addLimit(1, 0);
            q.addWhere(new DAL.WhereConds("in_id", ircNetwork.ToString()));

            ArrayList configSettings = db.executeSelect(q);

            _ircServer = (string) (((object[]) configSettings[0])[0]);
            _ircPort = (uint) ((object[]) configSettings[0])[1];

            _myNickname = (string) (((object[]) configSettings[0])[2]);
            _myPassword = (string) (((object[]) configSettings[0])[3]);
            _myUsername = (string) (((object[]) configSettings[0])[4]);
            _myRealname = (string) (((object[]) configSettings[0])[5]);

            this.logEvents = (bool) (((object[]) configSettings[0])[6]);

            _nickserv = (string) (((object[]) configSettings[0])[7]);

            if ( /*recieveWallops*/ true)
                this.connectionUserModes += 4;
            if ( /*invisible*/ true)
                this.connectionUserModes += 8;

            initialiseEventHandlers();
        }

        public IAL(string server, uint port, string nickname, string password, string username, string realname)
        {
            this.logEvents = true;
            this.floodProtectionWaitTime = 500;
            this.clientVersion = "Helpmebot IRC Access Layer 1.0";
            _networkId = 0;
            _ircServer = server;
            _ircPort = port;

            _myNickname = nickname;
            _myPassword = password;
            _myUsername = username;
            _myRealname = realname;

            initialiseEventHandlers();
        }

        ~IAL()
        {
            if (_tcpClient.Connected)
            {
                this.ircQuit();
            }
        }

        #endregion

        #region Methods

        public bool connect()
        {
            try
            {
                _tcpClient = new TcpClient(_ircServer, (int) _ircPort);

                Stream ircStream = _tcpClient.GetStream();
                _ircReader = new StreamReader(ircStream, Encoding.UTF8);
                _ircWriter = new StreamWriter(ircStream, Encoding.UTF8);


                _sendQ = new Queue(100);

                ThreadStart ircReaderThreadStart = _ircReaderThreadMethod;
                _ircReaderThread = new Thread(ircReaderThreadStart);

                ThreadStart ircWriterThreadStart = _ircWriterThreadMethod;
                _ircWriterThread = new Thread(ircWriterThreadStart);

                this.registerInstance();
                _ircReaderThread.Start();
                _ircWriterThread.Start();

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
            if ( !this.connected ) return;
            line = line.Replace("\n", " ");
            line = line.Replace("\r", " ");
            lock (this._sendQ)
            {
                this._sendQ.Enqueue(line.Trim());
            }
            this.messageCount++;
        }

        private void _sendPass(string password)
        {
            _sendLine("PASS " + password);
        }

        private void _sendNick(string nickname)
        {
            _sendLine("NICK " + nickname);
        }

        /// <summary>
        ///   Sends the USER command as part of connection registration
        /// </summary>
        /// <param name = "username">The client's username</param>
        /// <param name = "realname">The client's real name</param>
        private void _sendUser(string username, string realname)
        {
            _sendLine("USER " + username + " " + "*" + " * :" + realname);
        }

        private void registerConnection()
        {
            if (_myPassword != null)
                _sendPass(_myPassword);
            _sendUser(_myUsername, _myRealname);
            _sendNick(_myNickname);
        }

        private void assumeTakenNickname()
        {
            _sendNick(_myNickname + "_");
            if ( this._nickserv == string.Empty ) return;
            this.ircPrivmsg(this._nickserv, "GHOST " + this._myNickname + " " + this._myPassword);
            this.ircPrivmsg(this._nickserv, "RELEASE " + this._myNickname + " " + this._myPassword);
            this._sendNick(this._myNickname);
        }

        public void sendRawLine(string line)
        {
            _sendLine(line);
        }

        public void ircPong(string datapacket)
        {
            _sendLine("PONG " + datapacket);
        }

        public void ircPing(string datapacket)
        {
            _sendLine("PING " + datapacket);
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
                _sendLine("PRIVMSG " + destination + " :" + message.Substring(0, 400) + "...");
                this.ircPrivmsg(destination, "..." + message.Substring(400));
            }
            else
            {
                _sendLine("PRIVMSG " + destination + " :" + message);
            }
            Linker.instance().parseMessage(message, destination);
        }

        public void ircQuit(string message)
        {
            _sendLine("QUIT :" + message);
        }

        public void ircQuit()
        {
            _sendLine("QUIT");
        }

        public void ircJoin(string channel)
        {
            _sendLine("JOIN " + channel);
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
            _sendLine("MODE " + channel + " " + modeflags + " " + param);
        }

        public void ircMode(string channel, string flags)
        {
            this.ircMode(channel, flags, "");
        }

        public void ircPart(string channel, string message)
        {
            _sendLine("PART " + channel + " " + message);
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
            _sendLine("NAMES " + channel);
        }

        public void ircNames()
        {
            _sendLine("NAMES");
        }

        public void ircList()
        {
            _sendLine("LIST");
        }

        public void ircList(string channels)
        {
            _sendLine("LIST " + channels);
        }

        public void ircInvite(string nickname, string channel)
        {
            _sendLine("INVITE " + nickname + " " + channel);
        }

        public void ircKick(string channel, string user)
        {
            _sendLine("KICK " + channel + " " + user);
        }

        public void ircKick(string channel, string user, string reason)
        {
            _sendLine("KICK" + channel + " " + user + " :" + reason);
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
            _sendLine("NOTICE " + destination + " :" + message);
        }

        //TODO: Expand for network staff use
        public void ircMotd()
        {
            _sendLine("MOTD");
        }

        //TODO: Expand for network staff use
        public void ircLusers()
        {
            _sendLine("LUSERS");
        }

        //TODO: Expand for network staff use
        public void ircVersion()
        {
            _sendLine("VERSION");
        }

        public void ircStats(string query)
        {
            _sendLine("STATS " + query);
        }

        public void ircLinks(string mask)
        {
            _sendLine("LINKS " + mask);
        }

        public void ircTime()
        {
            _sendLine("TIME");
        }

        public void ircAdmin()
        {
            _sendLine("ADMIN");
        }

        public void ircInfo()
        {
            _sendLine("INFO");
        }

        public void ircWho(string mask)
        {
            _sendLine("WHO " + mask);
        }

        public void ircWhois(string mask)
        {
            _sendLine("WHOIS " + mask);
        }

        public void ircWhowas(string mask)
        {
            _sendLine("WHOWAS " + mask);
        }

        public void ircKill(string nickname, string comment)
        {
            _sendLine("KILL " + nickname + " :" + comment);
        }

        public void ircAway()
        {
            _sendLine("AWAY");
        }

        public void ircAway(string message)
        {
            _sendLine("AWAY :" + message);
        }

        public void ircIson(string nicklist)
        {
            _sendLine("ISON " + nicklist);
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

        private Thread _ircReaderThread;
        private Thread _ircWriterThread;

        private void _ircReaderThreadMethod()
        {
            bool threadIsAlive = true;
            do
            {
                try
                {
                    string line = _ircReader.ReadLine();
                    if (line == null)
                    {
                        // noop
                    }
                    else
                    {
                        if (this.dataRecievedEvent != null)
                            this.dataRecievedEvent(line);
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
            } while (threadIsAlive);

            Console.WriteLine("*** Reader thread died.");

            EventHandler temp = this.threadFatalError;
            if (temp != null)
            {
                temp(this, new EventArgs());
            }
        }

        private void _ircWriterThreadMethod()
        {
            bool threadIsAlive = true;
            do
            {
                try
                {
                    string line = null;
                    lock (_sendQ)
                    {
                        if (_sendQ.Count > 0)
                            line = (string) _sendQ.Dequeue();
                    }

                    if (line != null)
                    {
                        Logger.instance().addToLog("< " + line, Logger.LogTypes.IAL);
                        _ircWriter.WriteLine(line);
                        _ircWriter.Flush();
                        Thread.Sleep(this.floodProtectionWaitTime);
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
                    _sendQ.Clear();
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
            } while (threadIsAlive && _ircReaderThread.IsAlive);

            Console.WriteLine("*** Writer thread died.");

            EventHandler temp = this.threadFatalError;
            if (temp != null)
            {
                temp(this, new EventArgs());
            }
        }

        #endregion

        #region events

        public delegate void DataRecievedEventHandler(string data);

        public event DataRecievedEventHandler dataRecievedEvent;
        public event DataRecievedEventHandler unrecognisedDataRecievedEvent;

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

        private void initialiseEventHandlers()
        {
            this.dataRecievedEvent += this.ialDataRecievedEvent;
            this.unrecognisedDataRecievedEvent += IAL_unrecognisedDataRecievedEvent;
            this.connectionRegistrationRequiredEvent += registerConnection;
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
            this.errNicknameInUseEvent += assumeTakenNickname;
            this.errUnavailResource += this.ialErrUnavailResource;
            this.nameReplyEvent += this.ialNameReplyEvent;
            this.connectionRegistrationSucceededEvent += ialConnectionRegistrationSucceededEvent;
        }

        void IAL_unrecognisedDataRecievedEvent(string data)
        {
            this.log("DATA RECIEVED EVENT WITH DATA " + data);
        }

        void ialConnectionRegistrationSucceededEvent()
        {
            this.ircPrivmsg(_nickserv, "IDENTIFY " + _myNickname + " " + _myPassword);
            this.ircMode(_myNickname, "+Q");
        }

        private void ialErrUnavailResource()
        {
            if (_nickserv != string.Empty)
                assumeTakenNickname();

            else
                throw new NotImplementedException();
        }

        #region event handlers

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
                    this.ctcpReply(source.nickname, "VERSION", this.clientVersion);
                    break;
                case "TIME":
                    this.ctcpReply(source.nickname, "TIME", DateTime.Now.ToString());
                    break;
                case "PING":
                    this.ctcpReply(source.nickname, "PING", message.Split(' ')[1]);
                    break;
                case "FINGER":
                    this.ctcpReply(source.nickname, "FINGER", this.ircRealname + ", idle " + idleTime);
                    break;
                default:
                    break;
            }
        }

        private void ialPrivmsgEvent(User source, string destination, string message)
        {
            this.log("PRIVMSG EVENT FROM " + source + " TO " + destination + " MESSAGE " + message);
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
            if (source.nickname == ircNickname) this.channelList.Remove(channel);
        }

        private void ialJoinEvent(User source, string channel)
        {
            this.log("JOIN EVENT BY " + source + " INTO " + channel);
            if(source.nickname == ircNickname) this.channelList.Add(channel);
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

        private void ialDataRecievedEvent(string data)
        {
            Logger.instance().addToLog(data, Logger.LogTypes.IRC);

            char[] colonSeparator = {':'};

            string command, parameters;
            string messagesource = command = parameters = "";
            basicParser(data, ref messagesource, ref command, ref parameters);

            User source = new User();

            if (messagesource != null)
            {
                source = User.newFromString(messagesource, _networkId);
            }

            switch (command)
            {
                case "ERROR":
                    if (parameters.ToLower().Contains(":closing link"))
                    {
                        _tcpClient.Close();
                        _ircReaderThread.Abort();
                        _ircWriterThread.Abort();
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
                    this.joinEvent(source, parameters.Substring(1));
                    break;
                case "PART":
                    this.partEvent(source, parameters.Split(' ')[0],
                              parameters.Contains(new String(colonSeparator))
                                  ? parameters.Split(colonSeparator, 2)[1]
                                  : string.Empty);
                    break;
                case "TOPIC":
                    this.topicEvent(source, parameters.Split(' ')[0], parameters.Split(colonSeparator, 2)[1]);
                    break;
                case "INVITE":
                    this.inviteEvent(source, parameters.Split(' ')[0], parameters.Split(' ')[1].Substring(1));
                    break;
                case "KICK":
                    this.kickEvent(source, parameters.Split(' ')[0], parameters.Split(' ')[1],
                              parameters.Split(colonSeparator, 2)[1]);
                    break;
                case "PRIVMSG":
                    string message = parameters.Split(colonSeparator, 2)[1];
                    ASCIIEncoding asc = new ASCIIEncoding();
                    byte[] ctcp = {Convert.ToByte(1)};

                    string destination = parameters.Split(colonSeparator, 2)[0].Trim();
                    if (destination == this.ircNickname)
                    {
                        destination = source.nickname;
                    }

                    if (message.StartsWith(asc.GetString(ctcp)))
                    {
                        this.ctcpEvent(
                            source,
                            destination,
                            message.Trim(Convert.ToChar(Convert.ToByte(1)))
                            );
                    }
                    else
                    {
                        this.privmsgEvent(source, destination, message.Trim());
                    }
                    break;

                case "NOTICE":
                    string noticedestination = parameters.Split(colonSeparator, 2)[0].Trim();
                    if (noticedestination == this.ircNickname)
                    {
                        noticedestination = source.nickname;
                    }
                    this.noticeEvent(source, noticedestination, parameters.Split(colonSeparator, 2)[1]);
                    break;
                case "001":
                    this.connectionRegistrationSucceededEvent();
                    break;
                case "433":
                    this.errNicknameInUseEvent();
                    break;
                case "437":
                    this.errUnavailResource();
                    break;
                default:
                    break;
            }
        }

        #region parsers

        public static void basicParser(string line, ref string source, ref string command, ref string parameters)
        {
            char[] stringSplitter = {' '};
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
            if (this.logEvents)
            {
                Logger.instance().addToLog("<" + _networkId + ">" + message, Logger.LogTypes.IAL);
            }
        }

        #region IThreadedSystem Members

        public void stop()
        {
            this.ircQuit("Requested by controller");
            Thread.Sleep(5000);
            _ircWriterThread.Abort();
            _ircReaderThread.Abort();
        }

        public void registerInstance()
        {
            ThreadList.instance().register(this);
        }

        public string[] getThreadStatus()
        {
            string[] statuses = {
                                    "(" + _networkId + ") " + _ircServer + " READER:" + _ircReaderThread.ThreadState,
                                    "(" + _networkId + ") " + _ircServer + " WRITER:" + _ircWriterThread.ThreadState
                                };
            return statuses;
        }

        public event EventHandler threadFatalError;

        #endregion
    }
}