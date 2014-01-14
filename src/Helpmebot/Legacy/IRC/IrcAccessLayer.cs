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

namespace Helpmebot.Legacy.IRC
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.IRC.Events;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Threading;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///   IRC Access Layer - Provides an interface to IRC.
    /// </summary>
    public sealed class IrcAccessLayer : IThreadedSystem, IIrcAccessLayer
    {
        #region Readonly Fields

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
        /// Gets the server.
        /// </summary>
        private readonly string server;

        /// <summary>
        /// Gets the port.
        /// </summary>
        private readonly uint port;

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

        #endregion

        #region constructor/destructor

        /// <summary>
        /// Initialises a new instance of the <see cref="IrcAccessLayer"/> class.
        /// </summary>
        /// <param name="ircNetwork">
        /// The IRC network.
        /// </param>
        public IrcAccessLayer(uint ircNetwork)
        {
            // FIXME: Remove me!
            var logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.Log = logger.CreateChildLogger("Helpmebot.Legacy.IRC.IrcAccessLayer");

            this.networkId = ircNetwork; 

            DAL db = DAL.singleton();

            var q = new DAL.Select(
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

            this.server = (string)((object[])configSettings[0])[0];
            this.port = (uint)((object[])configSettings[0])[1];

            this.myNickname = (string)((object[])configSettings[0])[2];
            this.myPassword = (string)((object[])configSettings[0])[3];
            this.myUsername = (string)((object[])configSettings[0])[4];
            this.myRealname = (string)((object[])configSettings[0])[5];

            this.nickserv = (string)((object[])configSettings[0])[7];
        }

        /// <summary>
        /// Finalises an instance of the <see cref="IrcAccessLayer"/> class. 
        /// </summary>
        ~IrcAccessLayer()
        {
            if (this.tcpClient.Connected)
            {
                this.SendLine("QUIT");
            }
        }

        #endregion

        #region delegates

        /// <summary>
        /// The join event handler.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public delegate void JoinEventHandler(LegacyUser source, string channel);

        /// <summary>
        /// The invite event handler.
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
        public delegate void InviteEventHandler(LegacyUser source, string nickname, string channel);

        #endregion

        #region events

        /// <summary>
        /// The connection registration succeeded event.
        /// </summary>
        public event EventHandler ConnectionRegistrationSucceededEvent;

        /// <summary>
        /// The join event.
        /// </summary>
        public event JoinEventHandler JoinEvent;

        /// <summary>
        /// The invite event.
        /// </summary>
        public event InviteEventHandler InviteEvent;
        
        /// <summary>
        /// The private message event.
        /// </summary>
        public event EventHandler<PrivateMessageEventArgs> PrivateMessageEvent;

        /// <summary>
        /// The notice event.
        /// </summary>
        public event EventHandler<PrivateMessageEventArgs> NoticeEvent;
        
        #endregion

        #region properties

        /// <summary>
        /// Gets the nickname.
        /// </summary>
        public string Nickname
        {
            get
            {
                return this.myNickname;
            }
        }

        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        private ILogger Log { get; set; }

        #endregion

        #region Public Methods

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
                this.tcpClient = new TcpClient(this.server, (int)this.port);

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

                this.RegisterConnection();

                return true;
            }
            catch (SocketException ex)
            {
                this.Log.Error(ex.Message, ex);
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
        private void CtcpReply(string destination, string command, string parameters)
        {
            var asc = new ASCIIEncoding();
            byte[] ctcp = { Convert.ToByte(1) };
            this.IrcNotice(destination, asc.GetString(ctcp) + command.ToUpper() + " " + parameters + asc.GetString(ctcp));
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
            this.SendLine("NOTICE " + destination + " :" + message);
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
                        this.IrcDataReceivedEvent(new DataReceivedEventArgs(line));
                    }
                }
                catch (ThreadAbortException ex)
                {
                    threadIsAlive = false;
                    this.Log.Error(ex.Message, ex);
                }
                catch (IOException ex)
                {
                    threadIsAlive = false;
                    this.Log.Error(ex.Message, ex);
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex.Message, ex);
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
                        this.Log.Info("< " + line);
                        this.ircWriter.WriteLine(line);
                        this.ircWriter.Flush();
                        Thread.Sleep(500);
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
                    this.Log.Error(ex.Message, ex);
                    this.sendQ.Clear();
                }
                catch (IOException ex)
                {
                    threadIsAlive = false;
                    this.Log.Error(ex.Message, ex);
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex.Message, ex);
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

        #region event handlers

        /// <summary>
        /// The IRC CTCP event.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void IrcClientToClientEvent(LegacyUser user, string destination, string message)
        {
            this.Log.Debug("CTCP EVENT FROM " + user + " TO " + destination + " MESSAGE " + message);
            switch (message.Split(' ')[0].ToUpper())
            {
                case "VERSION":
                    this.CtcpReply(user.Nickname, "VERSION", "Unknown version");
                    break;
                case "TIME":
                    this.CtcpReply(user.Nickname, "TIME", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    break;
                case "PING":
                    this.CtcpReply(user.Nickname, "PING", message.Split(' ')[1]);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// The IRC data received event.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void IrcDataReceivedEvent(DataReceivedEventArgs e)
        {
            this.Log.Info(e.Data);

            char[] colonSeparator = { ':' };

            string command, parameters;
            string messagesource;
            string line = e.Data;
            char[] stringSplitter = { ' ' };
            string[] parseBasic;
            if (line.Substring(0, 1) == ":")
            {
                parseBasic = line.Split(stringSplitter, 3);
                messagesource = parseBasic[0].Substring(1);
                command = parseBasic[1];
                parameters = parseBasic[2];
            }
            else
            {
                parseBasic = line.Split(stringSplitter, 2);
                messagesource = null;
                command = parseBasic[0];
                parameters = parseBasic[1];
            }

            var source = new LegacyUser();

            if (messagesource != null)
            {
                source = LegacyUser.newFromString(messagesource, this.networkId);
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
                    this.SendLine("PONG " + parameters);
                    break;
                case "JOIN":
                    JoinEventHandler joinEvent = this.JoinEvent;
                    if (joinEvent != null)
                    {
                        joinEvent(source, parameters);
                    }
                    
                    if (source.Nickname == this.Nickname)
                    {
                        this.channelList.Add(parameters);
                    }

                    break;
                case "PART":
                    if (source.Nickname == this.Nickname)
                    {
                        this.channelList.Remove(parameters.Split(' ')[0]);
                    }
                    
                    break;
                case "INVITE":
                    var channel = parameters.Split(' ')[1].Substring(1);
                    var nickname = parameters.Split(' ')[0];

                    var ieh = this.InviteEvent;
                    if (ieh != null)
                    {
                        ieh(source, nickname, channel);
                    }

                    break;
                case "PRIVMSG":
                    string message = parameters.Split(colonSeparator, 2)[1];
                    var asc = new ASCIIEncoding();
                    byte[] ctcp = { Convert.ToByte(1) };

                    string destination = parameters.Split(colonSeparator, 2)[0].Trim();
                    if (destination == this.Nickname)
                    {
                        destination = source.Nickname;
                    }

                    if (message.StartsWith(asc.GetString(ctcp)))
                    {
                        this.IrcClientToClientEvent(
                            source,
                            destination,
                            message.Trim(Convert.ToChar(Convert.ToByte(1))));
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
                        noticedestination = source.Nickname;
                    }

                    var onNoticeEvent = this.NoticeEvent;
                    if (onNoticeEvent != null)
                    {
                        onNoticeEvent(
                            this,
                            new PrivateMessageEventArgs(
                                source,
                                noticedestination,
                                parameters.Split(colonSeparator, 2)[1]));
                    }
                    
                    break;
                case "001":
                    this.IrcPrivmsg(this.nickserv, "IDENTIFY " + this.myNickname + " " + this.myPassword);
                    string param = string.Empty;
                    this.SendLine(string.Format("MODE {0} +Q {1}", this.myNickname, param));

                    EventHandler temp = this.ConnectionRegistrationSucceededEvent;
                    if (temp != null)
                    {
                        temp(this, EventArgs.Empty);
                    }

                    break;
                case "004":
                    break;
                case "433":
                    this.AssumeTakenNickname();
                    break;
                case "437":
                    this.AssumeTakenNickname();
                    break;
                default:
                    this.Log.Debug("DATA RECIEVED EVENT WITH DATA " + e.Data);
                    break;
            }
        }

        #region IThreadedSystem Members

        /// <summary>
        /// The Stop.
        /// </summary>
        public void Stop()
        {
            this.SendLine("QUIT :" + "Requested by controller");
            Thread.Sleep(5000);
            this.ircWriterThread.Abort();
            this.ircReaderThread.Abort();
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
        /// The <see cref="string[]"/>.
        /// </returns>
        public string[] GetThreadStatus()
        {
            var reader = " READER:" + this.ircReaderThread.ThreadState;
            var writer = " WRITER:" + this.ircWriterThread.ThreadState;
            return new[] { reader, writer };
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
            if (!(this.tcpClient != null && this.tcpClient.Connected))
            {
                return;
            }

            line = line.Replace("\n", " ");
            line = line.Replace("\r", " ");
            lock (this.sendQ)
            {
                this.sendQ.Enqueue(line.Trim());
            }
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