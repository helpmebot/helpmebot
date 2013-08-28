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

namespace helpmebot6
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;

    class IrcProxyInstance  : Threading.IThreadedSystem
    {
        TcpClient _client;
        Thread _clientThread;
        bool threadActive = true;

        StreamReader _sr;
        StreamWriter _sw;

        public IrcProxyInstance(TcpClient client, string _password, IAL _baseIal)
        {
            this._client = client;
            this._password = _password;
            this._baseIal = _baseIal;

            _clientThread = new Thread(threadMethod);

            _clientThread.Start();
        }

        private void threadMethod()
        {
            try
            {
                _sr = new StreamReader(_client.GetStream());
                _sw = new StreamWriter(_client.GetStream());

                bool rcvdNick, rcvdUser, rcvdPass;
                rcvdPass = rcvdNick = rcvdUser = false;

                _sw.WriteLine(":helpmebot.srv.stwalkerster.net NOTICE * :*** Looking up your hostname...");
                _sw.WriteLine(":helpmebot.srv.stwalkerster.net NOTICE * :*** Checking Ident");
                _sw.WriteLine(":helpmebot.srv.stwalkerster.net NOTICE * :*** Found your hostname");
                _sw.WriteLine(":helpmebot.srv.stwalkerster.net NOTICE * :*** No Ident response");



                while (!(rcvdNick && rcvdUser && rcvdPass))
                {
                    string[] l = _sr.ReadLine().Split(' ');
                    if (l[0] == "NICK") rcvdNick = true;
                    if (l[0] == "USER") rcvdUser = true;
                    if (l[0] == "PASS")
                    {
                        if (l[1] == _password)
                            rcvdPass = true;
                    }
                }

                _sw.WriteLine(":irc.helpmebot.org.uk 001 " + _baseIal.ircNickname + " :Welcome to the Helpmebot IRC Gateway.");
                _sw.WriteLine(":irc.helpmebot.org.uk 002 " + _baseIal.ircNickname + " :Your Host is helpmebot.srv.stwalkerster.net, running version Helpmebot/6.0");
                _sw.WriteLine(":irc.helpmebot.org.uk 003 " + _baseIal.ircNickname + " :This server was created " + DateTime.Now.ToString());
                _sw.WriteLine(":irc.helpmebot.org.uk 004 " + _baseIal.ircNickname + " " + _baseIal.ServerInfo);
                _sw.Flush();

                foreach (var c in _baseIal.activeChannels)
                {
                    _sw.WriteLine(":" + _baseIal.myIdentity + " JOIN " + c);
                    _sw.Flush();
                    _baseIal.ircNames(c);
                    _baseIal.ircTopic(c);
                }

                



                _sw.Flush();
                _baseIal.dataRecievedEvent += baseIalDataRecievedEvent;

                while (_client.Connected)
                {
                    string line = _sr.ReadLine();
                    string source = null;
                    string command = null;
                    string parameters = null;
                    IAL.basicParser(line, ref source, ref command, ref parameters);

                    if (command == "QUIT")
                    {
                        _sr.Close();
                        _baseIal.dataRecievedEvent -= baseIalDataRecievedEvent;
                        break;
                    }

                    _baseIal.sendRawLine(line);
                }
            }
            catch (ThreadAbortException)
            {

            }
            finally
            {
                _client.Close();
            }
        }

        void baseIalDataRecievedEvent(string data)
        {
            _sw.WriteLine(data);
            _sw.Flush();
        }

        void Threading.IThreadedSystem.stop()
        {
            threadActive = false;
            Thread.Sleep(3000);
            _clientThread.Abort();
        }

        void Threading.IThreadedSystem.registerInstance()
        {
            Threading.ThreadList.instance().register(this);
        }

        string[] Threading.IThreadedSystem.getThreadStatus()
        {
            string[] x = { _clientThread.ThreadState.ToString() };
            return x;
        }

        public event EventHandler threadFatalError;
        private string _password;
        private IAL _baseIal;
    }
}
