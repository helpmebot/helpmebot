/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using helpmebot6.Threading;

namespace helpmebot6
{
    /// <summary>
    /// IRC Access Layer
    /// 
    /// Provides an interface to IRC.
    /// </summary>
    public class IAL : IThreadedSystem
    {
        #region internal variables

        string _myNickname;
        string _myUsername;
        string _myRealname;
        string _myPassword;

        string _ircServer;
        uint _ircPort;

        string _nickserv;

        TcpClient _tcpClient;
        StreamReader _ircReader;
        StreamWriter _ircWriter;

        Queue _sendQ;

        int _floodProtectionWaitTime = 500;

        int _connectionUserModes = 0;

        string _version = "Helpmebot IRC Access Layer 1.0";

        int _messageCount = 0;

        private System.Collections.Hashtable namesList = new Hashtable( );

        DateTime lastMessage = DateTime.Now;

        private uint _networkId = 0;

        private bool _logEvents = true;
        #endregion

        #region properties


        public string ClientVersion
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }

        public bool Connected
        {
            get
            {
                if ( this._tcpClient != null )
                {
                    return this._tcpClient.Connected;
                }
                else
                {
                    return false;
                }
            }
        }

        public string IrcNickname
        {
            get
            {
                return _myNickname;
            }
            set
            {
            }
        }
        public string IrcUsername
        {
            get
            {
                return _myUsername;
            }
        }
        public string IrcRealname
        {
            get
            {
                return _myRealname;
            }
        }

        public string IrcServer
        {
            get
            {
                return _ircServer;
            }
        }
        public uint IrcPort
        {
            get
            {
                return _ircPort;
            }
        }

        public int FloodProtectionWaitTime
        {
            get
            {
                return _floodProtectionWaitTime;
            }
            set
            {
                _floodProtectionWaitTime = value;
            }
        }

        /// <summary>
        /// +4 if recieving wallops, +8 if invisible
        /// </summary>
        public int ConnectionUserModes
        {
            get
            {
                return _connectionUserModes;
            }
        }

        public int MessageCount
        {
            get
            {
                return _messageCount;
            }
        }

        public TimeSpan idleTime
        {
            get
            {
                return DateTime.Now.Subtract( lastMessage );
            }
        }

        public bool LogEvents
        {
            get
            {
                return _logEvents;
            }
        }
        #endregion

        #region constructor/destructor

        public IAL( uint ircNetwork )
        {
            _networkId = ircNetwork;

            DAL db = DAL.Singleton( );

            string[ ] selects = { "in_host", "in_port", "in_nickname", "in_password", "in_username", "in_realname", "in_log", "in_nickserv" };
            string[ ] wheres = { "in_id = " + ircNetwork };
            ArrayList configSettings = db.Select( selects , "ircnetwork" , new DAL.join[ 0 ] , wheres , new string[ 0 ] , new DAL.order[ 0 ] , new string[ 0 ] , 1 , 0 );

            _ircServer = (string)(((object[])configSettings[ 0 ])[ 0 ]);
            _ircPort = (uint)( (object[ ])configSettings[ 0 ] )[ 1 ];

            _myNickname = (string)( ( (object[ ])configSettings[ 0 ] )[ 2 ] );
            _myPassword = (string)( ( (object[ ])configSettings[ 0 ] )[ 3 ] );
            _myUsername = (string)( ( (object[ ])configSettings[ 0 ] )[ 4 ] );
            _myRealname = (string)( ( (object[ ])configSettings[ 0 ] )[ 5 ] );

            _logEvents = (bool)( ( (object[ ])configSettings[ 0 ] )[ 6 ] );

            _nickserv = (string)( ( (object[ ])configSettings[ 0 ] )[ 7 ] );

            if( /*recieveWallops*/ true )
                _connectionUserModes += 4;
            if( /*invisible*/ true )
                _connectionUserModes += 8;

            initialiseEventHandlers( );
        }

        public IAL( string server, uint port, string nickname, string password, string username, string realname )
        {
            _networkId = 0;
            _ircServer = server;
            _ircPort = port;

            _myNickname = nickname;
            _myPassword = password;
            _myUsername = username;
            _myRealname = realname;

            initialiseEventHandlers( );
        }

        ~IAL( )
        {
            if( this._tcpClient.Connected  )
            {
                this.IrcQuit( );
            }
        }

        #endregion
        
        #region Methods

        public bool Connect( )
        {
            try
            {
                _tcpClient = new TcpClient( _ircServer, (int)_ircPort );

                Stream _IrcStream = _tcpClient.GetStream( );
                _ircReader = new StreamReader( _IrcStream );
                _ircWriter = new StreamWriter( _IrcStream );


                _sendQ = new Queue( 100 );

                ThreadStart _ircReaderThreadStart = new ThreadStart( _ircReaderThreadMethod );
                _ircReaderThread = new Thread( _ircReaderThreadStart );
                
                ThreadStart _ircWriterThreadStart = new ThreadStart( _ircWriterThreadMethod );
                _ircWriterThread = new Thread( _ircWriterThreadStart );

                RegisterInstance( );
                _ircReaderThread.Start( );
                _ircWriterThread.Start( );

                ConnectionRegistrationRequiredEvent( );

                return true;
            }
            catch ( SocketException ex )
            {
                GlobalFunctions.ErrorLog( ex );
                return false;
            }
        }

        void _sendLine( string line )
        {
            if ( this.Connected )
            {
                line = line.Replace( "\n", " " );
                line = line.Replace( "\r", " " );
                lock (_sendQ)
                {
                    _sendQ.Enqueue(line.Trim());
                }
                _messageCount++;
                
            }
        }

        void _sendPass( string password )
        {
            _sendLine( "PASS " + password );
        }

        void _sendNick( string nickname )
        {
            _sendLine( "NICK " + nickname );
        }

        /// <summary>
        /// Sends the USER command as part of connection registration
        /// </summary>
        /// <param name="username">The client's username</param>
        /// <param name="mode">The connection user modes: bitmask, bit 3 (8) is invisible, bit 2 (4) recieves wallops</param>
        /// <param name="realname">The client's real name</param>
        void _sendUser( string username, int mode, string realname )
        {
            _sendLine( "USER " + username + " " + /*mode.ToString()*/ "*" + " * :" + realname );
        }

        void registerConnection( )
        {
            if ( _myPassword != null )
                _sendPass( _myPassword );
            _sendUser( _myUsername, _connectionUserModes, _myRealname );
            _sendNick( _myNickname );
        }

        void assumeTakenNickname( )
        {
            _sendNick( _myNickname + "_" );
            if( _nickserv != string.Empty )
            {
                this.IrcPrivmsg( _nickserv, "GHOST " + _myNickname + " " + _myPassword );
                this.IrcPrivmsg( _nickserv, "RELEASE " + _myNickname + " " + _myPassword );
                _sendNick( _myNickname );
            }
        }

        public void SendRawLine(string line)
        {
            _sendLine(line);
        }

        public void IrcPong( string datapacket )
        {
            _sendLine( "PONG " + datapacket );
        }

        public void IrcPing( string datapacket )
        {
            _sendLine( "PING " + datapacket );
        }

        /// <summary>
        /// Sends a private message
        /// </summary>
        /// <param name="Destination">The destination of the private message.</param>
        /// <param name="Message">The message text to be sent</param>
        public void IrcPrivmsg( string Destination, string Message )
        {
            if ( Message.Length > 400 )
            {
                _sendLine( "PRIVMSG " + Destination + " :" + Message.Substring( 0, 400 ) + "..." );
                IrcPrivmsg( Destination, "..." + Message.Substring( 400 ) );
            }
            else
            {
                _sendLine( "PRIVMSG " + Destination + " :" + Message );
            }
            Linker.Instance( ).ParseMessage( Message , Destination );
        }

        public void IrcQuit( string message )
        {
            _sendLine( "QUIT :" + message );
        }
        public void IrcQuit( )
        {
            _sendLine( "QUIT" );
        }

        public void IrcJoin( string channel )
        {
            _sendLine( "JOIN " + channel );
        }
        public void IrcJoin( string[ ] channels )
        {
            foreach ( string channel in channels )
            {
                this.IrcJoin( channel );
            }
        }

        public void IrcMode( string channel, string modeflags, string param )
        {
            _sendLine( "MODE " + channel + " " + modeflags + " " + param );
        }
        public void IrcMode( string channel, string flags )
        {
            this.IrcMode( channel, flags, "" );
        }

        public void IrcPart( string channel, string message )
        {
            _sendLine( "PART " + channel + " " + message );
        }
        public void IrcPart( string channel )
        {
            IrcPart( channel, "" );
        }
        public void PartAllChannels( )
        {
            IrcJoin( "0" );
        }

        public void IrcNames( string channel )
        {
            _sendLine( "NAMES " + channel );
        }
        public void IrcNames( )
        {
            _sendLine( "NAMES" );
        }

        public void IrcList( )
        {
            _sendLine( "LIST" );
        }
        public void IrcList( string channels )
        {
            _sendLine( "LIST " + channels );
        }

        public void IrcInvite( string nickname, string channel )
        {
            _sendLine( "INVITE " + nickname + " " + channel );
        }

        public void IrcKick( string channel, string user )
        {
            _sendLine( "KICK " + channel + " " + user );
        }
        public void IrcKick( string channel, string user, string reason )
        {
            _sendLine( "KICK" + channel + " " + user + " :" + reason );
        }

        public void CtcpReply( string destination, string command, string parameters )
        {
            ASCIIEncoding asc = new ASCIIEncoding( );
            byte[ ] ctcp = { Convert.ToByte( 1 ) };
            IrcNotice( destination, asc.GetString( ctcp ) + command.ToUpper( ) + " " + parameters + asc.GetString( ctcp ) );
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

        public static string wrapCTCP( string command , string parameters )
        {
            ASCIIEncoding asc = new ASCIIEncoding( );
            byte[ ] ctcp = { Convert.ToByte( 1 ) };
            return ( asc.GetString( ctcp ) + command.ToUpper( ) + ( parameters == string.Empty ? "" : " " + parameters ) + asc.GetString( ctcp ) );

        }

        public void IrcNotice( string destination, string message )
        {
            _sendLine( "NOTICE " + destination + " :" + message );
        }

        //TODO: Expand for network staff use
        public void IrcMotd( )
        {
            _sendLine( "MOTD" );
        }
        //TODO: Expand for network staff use
        public void IrcLusers( )
        {
            _sendLine( "LUSERS" );
        }

        //TODO: Expand for network staff use
        public void IrcVersion( )
        {
            _sendLine( "VERSION" );
        }

        public void IrcStats( string query )
        {
            _sendLine( "STATS " + query );
        }

        public void IrcLinks( string mask )
        {
            _sendLine( "LINKS " + mask );
        }

        public void IrcTime( )
        {
            _sendLine( "TIME" );
        }

        public void IrcAdmin( )
        {
            _sendLine( "ADMIN" );
        }

        public void IrcInfo( )
        {
            _sendLine( "INFO" );
        }

        public void IrcWho( string mask )
        {
            _sendLine( "WHO " + mask );
        }

        public void IrcWhois( string mask )
        {
            _sendLine( "WHOIS " + mask );
        }
        public void IrcWhowas( string mask )
        {
            _sendLine( "WHOWAS " + mask );
        }

        public void IrcKill( string nickname, string comment )
        {
            _sendLine( "KILL " + nickname + " :" + comment );
        }

        public void IrcAway( )
        {
            _sendLine( "AWAY" );
        }

        public void IrcAway( string message )
        {
            _sendLine( "AWAY :" + message );
        }

        public void IrcIson( string nicklist )
        {
            _sendLine( "ISON " + nicklist );
        }

        /// <summary>
        /// Compares the channel name against the valid channel name settings returned by the IRC server on connection
        /// </summary>
        /// <param name="ChannelName">Channel name to check</param>
        /// <returns>Boolean true if provided channel name is valid</returns>
        public bool isValidChannelName( string ChannelName )
        {
            // TODO: make better!
            if( ChannelName.StartsWith( "#" ) )
                return true;
            else
                return false;
        }

        

        /// <summary>
        /// checks if nickname is on channel
        /// </summary>
        /// <param name="channel">channel to check</param>
        /// <param name="nickname">nickname to check</param>
        /// <returns>1 if nickname is on channel
        /// 0 if nickname is not on channel
        /// -1 if it cannot be checked at the moment</returns>
        public int isOnChannel( string channel , string nickname )
        {
            if( namesList.ContainsKey( channel ) )
            {
                if( ( (ArrayList)namesList[ channel ] ).Contains( nickname ) )
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                IrcNames( channel );
                return -1;
            }
        }
        #endregion

        #region Threads
        Thread _ircReaderThread;
        Thread _ircWriterThread;

        void _ircReaderThreadMethod( )
        {
            bool _ThreadIsAlive = true;
            do
            {
                try
                {
                    string line = _ircReader.ReadLine( );
                    if ( line == null )
                    {
                        // noop
                    }
                    else
                    {
                        if ( this.DataRecievedEvent != null )
                            DataRecievedEvent( line );
                    }
                }
                catch ( ThreadAbortException ex )
                {
                    _ThreadIsAlive = false;
                    GlobalFunctions.ErrorLog( ex );
                }
                catch ( IOException ex )
                {
                    _ThreadIsAlive = false;
                    GlobalFunctions.ErrorLog( ex );
                }
                catch ( Exception ex )
                {
                    GlobalFunctions.ErrorLog( ex );
                }
            }
            while ( _ThreadIsAlive );

            Console.WriteLine( "*** Reader thread died." );
            Helpmebot6.Stop( );
        }

        void _ircWriterThreadMethod( )
        {
            bool _ThreadIsAlive = true;
            do
            {
                try
                {
                    string line = null;
                    lock (_sendQ)
                    {
                        if (_sendQ.Count > 0)
                            line = (string)_sendQ.Dequeue();
                    }

                    if (line != null)
                    {
                        Logger.Instance( ).addToLog( "< " + line , Logger.LogTypes.IAL );
                        _ircWriter.WriteLine( line );
                        _ircWriter.Flush( );
                        Thread.Sleep( this.FloodProtectionWaitTime );
                    }
                    else
                    {
                        // wait a short while before rechecking
                        Thread.Sleep( 100 );
                    }
                }
                catch ( ThreadAbortException ex )
                {
                    _ThreadIsAlive = false;
                    GlobalFunctions.ErrorLog( ex );
                    _sendQ.Clear( );
                }
                catch ( IOException ex )
                {
                    _ThreadIsAlive = false;
                    GlobalFunctions.ErrorLog( ex );
                }
                catch ( Exception ex )
                {
                    GlobalFunctions.ErrorLog( ex );
                }
            }
            while ( _ThreadIsAlive && _ircReaderThread.IsAlive);

            Console.WriteLine( "*** Writer thread died." );
            Helpmebot6.Stop( );
        }
        #endregion

        #region events
        public delegate void DataRecievedEventHandler( string data );
        public event DataRecievedEventHandler DataRecievedEvent;

        public delegate void ConnectionRegistrationEventHandler( );
        event ConnectionRegistrationEventHandler ConnectionRegistrationRequiredEvent;
        public event ConnectionRegistrationEventHandler ConnectionRegistrationSucceededEvent;

        public delegate void PingEventHandler( string datapacket );
        public event PingEventHandler PingEvent;

        public delegate void NicknameChangeEventHandler( string oldnick, string newnick );
        public event NicknameChangeEventHandler NicknameChangeEvent;

        public delegate void ModeChangeEventHandler( User source, string subject, string flagchanges, string parameter );
        public event ModeChangeEventHandler ModeChangeEvent;

        public delegate void QuitEventHandler( User source, string message );
        public event QuitEventHandler QuitEvent;

        public delegate void JoinEventHandler( User source, string channel );
        public event JoinEventHandler JoinEvent;

        public delegate void PartEventHandler( User source, string channel, string message );
        public event PartEventHandler PartEvent;

        public delegate void TopicEventHandler( User source, string channel, string topic );
        public event TopicEventHandler TopicEvent;

        public delegate void InviteEventHandler( User source, string nickname, string channel );
        public event InviteEventHandler InviteEvent;

        public delegate void KickEventHandler( User source, string channel, string nick, string message );
        public event KickEventHandler KickEvent;

        public delegate void PrivmsgEventHandler( User source, string destination, string message );
        public event PrivmsgEventHandler PrivmsgEvent;
        public event PrivmsgEventHandler CtcpEvent;
        public event PrivmsgEventHandler NoticeEvent;

        public delegate void IrcEventHandler( );
        public event IrcEventHandler Err_NicknameInUseEvent;
        public event IrcEventHandler Err_UnavailResource;

        public delegate void NameReplyEventHandler(string channel, string[] names);
        public event NameReplyEventHandler NameReplyEvent;

        #endregion

        private void initialiseEventHandlers( )
        {
            this.DataRecievedEvent += new DataRecievedEventHandler( IAL_DataRecievedEvent );
            this.ConnectionRegistrationRequiredEvent += new ConnectionRegistrationEventHandler( registerConnection );
            this.PingEvent += new PingEventHandler( IrcPong );
            this.NicknameChangeEvent += new NicknameChangeEventHandler( IAL_NicknameChangeEvent );
            this.QuitEvent += new QuitEventHandler( IAL_QuitEvent );
            this.JoinEvent += new JoinEventHandler( IAL_JoinEvent );
            this.PartEvent += new PartEventHandler( IAL_PartEvent );
            this.TopicEvent += new TopicEventHandler( IAL_TopicEvent );
            this.ModeChangeEvent += new ModeChangeEventHandler( IAL_ModeChangeEvent );
            this.InviteEvent += new InviteEventHandler( IAL_InviteEvent );
            this.KickEvent += new KickEventHandler( IAL_KickEvent );
            this.PrivmsgEvent += new PrivmsgEventHandler( IAL_PrivmsgEvent );
            this.CtcpEvent += new PrivmsgEventHandler( IAL_CtcpEvent );
            this.NoticeEvent += new PrivmsgEventHandler( IAL_NoticeEvent );
            this.Err_NicknameInUseEvent += new IrcEventHandler( assumeTakenNickname );
            this.Err_UnavailResource += new IrcEventHandler( IAL_Err_UnavailResource );
        }

        void IAL_Err_UnavailResource( )
        {
            if( this._nickserv != string.Empty )
                assumeTakenNickname( );

            else
                throw new NotImplementedException( );
        }


        #region event handlers

        void IAL_NameReplyEvent( string channel , string[ ] names )
        {
            if( namesList.ContainsKey( channel ) )
            {
                foreach( string name in names )
                {
                    ArrayList channelNamesList = (ArrayList)namesList[ channel ];
                    string newName = name.Trim( '@' , '+' );
                    if( !channelNamesList.Contains( newName ) )
                        channelNamesList.Add( newName );
                }

            }
        }

        void IAL_NoticeEvent( User source, string destination, string message )
        {
            Log( "NOTICE EVENT FROM " + source.ToString() + " TO " + destination + " MESSAGE " + message );
        }

        void IAL_CtcpEvent( User source, string destination, string message )
        {
            Log("CTCP EVENT FROM " + source.ToString() + " TO " + destination + " MESSAGE " + message );
            switch ( message.Split(' ')[0] )
            {
                case "VERSION":
                    CtcpReply( source.Nickname, "VERSION", this.ClientVersion );
                    break;
                case "TIME":
                    CtcpReply( source.Nickname , "TIME" , DateTime.Now.ToString( ) );
                    break;
                case "PING":
                    CtcpReply( source.Nickname , "PING" , message.Split(' ')[1] );
                    break;
                case "FINGER":
                    CtcpReply( source.Nickname , "FINGER" , this.IrcRealname + ", idle " + this.idleTime.ToString( ) );
                    break;
                default:
                    break;
            }
        }

        void IAL_PrivmsgEvent( User source, string destination, string message )
        {
            Log("PRIVMSG EVENT FROM " + source.ToString() + " TO " + destination + " MESSAGE " + message );
        }

        void IAL_KickEvent( User source, string channel, string nick, string message )
        {
            Log("KICK FROM " + channel + " BY " + source.ToString() + " AFFECTED " + nick + " REASON " + message );
        }

        void IAL_InviteEvent( User source, string nickname, string channel )
        {
            Log("INVITE FROM " + source.ToString() + " TO " + nickname + " CHANNEL " + channel );
        }

        void IAL_ModeChangeEvent( User source, string subject, string flagchanges, string parameter )
        {
            Log("MODE CHANGE BY " + source.ToString() + " ON " + subject + " CHANGES " + flagchanges + " PARAMETER " + parameter );
        }

        void IAL_TopicEvent( User source, string channel, string topic )
        {
            Log("TOPIC CHANGED BY " + source.ToString() + " IN " + channel + " TOPIC " + topic );
        }

        void IAL_PartEvent( User source, string channel, string message )
        {
            Log("PART BY " + source.ToString() + " FROM " + channel + " MESSAGE " + message );
        }

        void IAL_JoinEvent( User source, string channel )
        {
            Log("JOIN EVENT BY " + source.ToString( ) + " INTO " + channel );
        }

        void IAL_QuitEvent( User source, string message )
        {
            Log("QUIT BY " + source.ToString( ) + " MESSAGE " + message );
        }

        void IAL_NicknameChangeEvent( string oldnick, string newnick )
        {
            Log("NICK CHANGE BY " + oldnick + " TO " + newnick );
        }
        #endregion

        void IAL_DataRecievedEvent( string data )
        {
            Logger.Instance( ).addToLog( data , Logger.LogTypes.IRC );
            
            char[ ] colonSeparator = { ':' };

            string messagesource, command, parameters;
            messagesource = command = parameters = "";
            basicParser( data, ref messagesource, ref command, ref parameters );

            User source = new User();

            if ( messagesource != null )
            {
                source = User.newFromString( messagesource );
            }

            switch ( command )
            {
                case "ERROR":
                    if ( parameters.ToLower( ).Contains( ":closing link" ) )
                    {
                        _tcpClient.Close( );
                        _ircReaderThread.Abort( );
                        _ircWriterThread.Abort( );
                    }
                    break;
                case "PING":
                    PingEvent( parameters );
                    break;
                case "NICK":
                    NicknameChangeEvent( source.Nickname, parameters.Substring( 1 ) );
                    break;
                case "MODE":
                    try
                    {
                        string subject = parameters.Split( ' ' )[ 0 ];
                        string flagchanges = parameters.Split( ' ' )[ 1 ];
                        string param = "";
                        if ( parameters.Split( ' ' ).Length > 2 )
                            param = parameters.Split( ' ' )[ 2 ];
                        else
                            param = "";

                        ModeChangeEvent( source, subject, flagchanges, param );
                    }
                    catch ( NullReferenceException ex )
                    {
                        GlobalFunctions.ErrorLog( ex );
                    }
                    break;
                case "QUIT":
                    QuitEvent( source, parameters );
                    break;
                case "JOIN":
                    JoinEvent( source, parameters.Substring(1) );
                    break;
                case "PART":
                    PartEvent( source, parameters.Split( ' ' )[ 0 ], parameters.Contains( new String( colonSeparator ) ) ? parameters.Split( colonSeparator, 2 )[ 1 ] : string.Empty );
                    break;
                case "TOPIC":
                    TopicEvent( source, parameters.Split( ' ' )[ 0 ], parameters.Split( colonSeparator, 2 )[ 1 ] );
                    break;
                case "INVITE":
                    InviteEvent( source, parameters.Split( ' ' )[ 0 ], parameters.Split( ' ' )[ 1 ].Substring( 1 ) );
                    break;
                case "KICK":
                    KickEvent( source, parameters.Split( ' ' )[ 0 ], parameters.Split( ' ' )[ 1 ], parameters.Split( colonSeparator, 2 )[ 1 ] );
                    break;
                case "PRIVMSG":
                    string message = parameters.Split( colonSeparator, 2 )[ 1 ];
                    ASCIIEncoding asc = new ASCIIEncoding( );
                    byte[ ] ctcp = { Convert.ToByte( 1 ) };

                    string destination = parameters.Split( colonSeparator, 2 )[ 0 ].Trim( );
                    if ( destination == this.IrcNickname )
                    {
                        destination = source.Nickname;
                    }

                    if ( message.StartsWith( asc.GetString( ctcp ) ) )
                    {
                        CtcpEvent(
                            source,
                            destination,
                            message.Trim( Convert.ToChar( Convert.ToByte( 1 ) ) )
                            );
                    }
                    else
                    {
                        PrivmsgEvent( source, destination, message.Trim() );
                    }
                    break;

                case "NOTICE":
                    string noticedestination = parameters.Split( colonSeparator, 2 )[ 0 ].Trim( );
                    if ( noticedestination == this.IrcNickname )
                    {
                        noticedestination = source.Nickname;
                    }
                    NoticeEvent( source, noticedestination, parameters.Split( colonSeparator, 2 )[ 1 ] );
                    break;
                case "001":
                    ConnectionRegistrationSucceededEvent( );
                    break;
                case "433":
                    Err_NicknameInUseEvent( );
                    break;
                case "437":
                    Err_UnavailResource( );
                    break;
                default:
                    break;
            }
        }

        #region parsers

        private static void basicParser( string line, ref string source, ref string command, ref string parameters )
        {
            char[ ] stringSplitter = { ' ' };
            string[ ] parseBasic;
            if ( line.Substring( 0, 1 ) == ":" )
            {
                parseBasic = line.Split( stringSplitter, 3 );
                source = parseBasic[ 0 ].Substring( 1 );
                command = parseBasic[ 1 ];
                parameters = parseBasic[ 2 ];
            }
            else
            {
                parseBasic = line.Split( stringSplitter, 2 );
                source = null;
                command = parseBasic[ 0 ];
                parameters = parseBasic[ 1 ];
            }
        }
        #endregion

        void Log( string message )
        {
            if( this.LogEvents )
            {
                Logger.Instance( ).addToLog( "<" + _networkId + ">" + message, Logger.LogTypes.IAL );
            }
        }

        #region IThreadedSystem Members

        public void Stop( )
        {
            throw new NotImplementedException( );
        }

        public void RegisterInstance( )
        {
            ThreadList.instance( ).register( this );
        }

        public string[ ] getThreadStatus( )
        {
            string[ ] statuses = {
                    "(" + this._networkId + ") " + this._ircServer + " READER:" + this._ircReaderThread.ThreadState,
                    "(" + this._networkId + ") " + this._ircServer + " WRITER:" + this._ircWriterThread.ThreadState
                                };
            return statuses;
        }

        #endregion
    }
}