// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcProxyInstance.cs" company="Helpmebot Development Team">
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
//   Defines the IrcProxyInstance type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;

    using Helpmebot.IRC.Events;
    using Helpmebot.Threading;

    class IrcProxyInstance  : IThreadedSystem
    {
        TcpClient _client;
        Thread _clientThread;
        bool threadActive = true;

        StreamReader _sr;
        StreamWriter _sw;

        public IrcProxyInstance(TcpClient client, string _password, IrcAccessLayer _baseIal)
        {
            this._client = client;
            this._password = _password;
            this._baseIal = _baseIal;

            this._clientThread = new Thread(this.threadMethod);

            this._clientThread.Start();
        }

        private void threadMethod()
        {
            try
            {
                this._sr = new StreamReader(this._client.GetStream());
                this._sw = new StreamWriter(this._client.GetStream());

                bool rcvdNick, rcvdUser, rcvdPass;
                rcvdPass = rcvdNick = rcvdUser = false;

                this._sw.WriteLine(":helpmebot.srv.stwalkerster.net NOTICE * :*** Looking up your hostname...");
                this._sw.WriteLine(":helpmebot.srv.stwalkerster.net NOTICE * :*** Checking Ident");
                this._sw.WriteLine(":helpmebot.srv.stwalkerster.net NOTICE * :*** Found your hostname");
                this._sw.WriteLine(":helpmebot.srv.stwalkerster.net NOTICE * :*** No Ident response");



                while (!(rcvdNick && rcvdUser && rcvdPass))
                {
                    string[] l = this._sr.ReadLine().Split(' ');
                    if (l[0] == "NICK") rcvdNick = true;
                    if (l[0] == "USER") rcvdUser = true;
                    if (l[0] == "PASS")
                    {
                        if (l[1] == this._password)
                            rcvdPass = true;
                    }
                }

                this._sw.WriteLine(":irc.helpmebot.org.uk 001 " + this._baseIal.Nickname + " :Welcome to the Helpmebot IRC Gateway.");
                this._sw.WriteLine(":irc.helpmebot.org.uk 002 " + this._baseIal.Nickname + " :Your Host is helpmebot.srv.stwalkerster.net, running version Helpmebot/6.0");
                this._sw.WriteLine(":irc.helpmebot.org.uk 003 " + this._baseIal.Nickname + " :This server was created " + DateTime.Now.ToString());
                this._sw.WriteLine(":irc.helpmebot.org.uk 004 " + this._baseIal.Nickname + " " + this._baseIal.ServerInfo);
                this._sw.Flush();

                foreach (var c in this._baseIal.ActiveChannels)
                {
                    this._sw.WriteLine(":" + this._baseIal.MyIdentity + " JOIN " + c);
                    this._sw.Flush();
                    this._baseIal.IrcNames(c);
                    this._baseIal.IrcTopic(c);
                }

                this._sw.Flush();
                this._baseIal.DataReceivedEvent += this.baseIalDataRecievedEvent;

                while (this._client.Connected)
                {
                    string line = this._sr.ReadLine();
                    string source = null;
                    string command = null;
                    string parameters = null;
                    IrcAccessLayer.BasicParser(line, ref source, ref command, ref parameters);

                    if (command == "QUIT")
                    {
                        this._sr.Close();
                        this._baseIal.DataReceivedEvent -= this.baseIalDataRecievedEvent;
                        break;
                    }

                    this._baseIal.SendRawLine(line);
                }
            }
            catch (ThreadAbortException)
            {

            }
            finally
            {
                this._client.Close();
            }
        }

        void baseIalDataRecievedEvent(object sender, DataReceivedEventArgs e)
        {
            this._sw.WriteLine(e.Data);
            this._sw.Flush();
        }

        void IThreadedSystem.Stop()
        {
            this.threadActive = false;
            Thread.Sleep(3000);
            this._clientThread.Abort();
        }

        void IThreadedSystem.RegisterInstance()
        {
            ThreadList.instance().register(this);
        }

        string[] IThreadedSystem.GetThreadStatus()
        {
            string[] x = { this._clientThread.ThreadState.ToString() };
            return x;
        }

        public event EventHandler ThreadFatalErrorEvent;
        private string _password;
        private IrcAccessLayer _baseIal;
    }
}
