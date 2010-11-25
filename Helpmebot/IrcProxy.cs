using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace helpmebot6
{
    class IrcProxy : Threading.IThreadedSystem
    {
        public IrcProxy(IAL baseIrcAccessLayer, int port, string password)
        {
            _listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));

            _baseIal = baseIrcAccessLayer;
            this._password = password;

            this.registerInstance();

            _t = new Thread(listenerThread);
            _t.Start();
        }

        private readonly IAL _baseIal;
        private readonly string _password;
        private readonly TcpListener _listener;
        private readonly Thread _t;

        StreamReader _sr;
        StreamWriter _sw;

        private void listenerThread()
        {
            try
            {
                _listener.Start();

                while (true)
                {
                    if (!_listener.Pending())
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    TcpClient client = _listener.AcceptTcpClient();
                    _sr = new StreamReader(client.GetStream());
                    _sw = new StreamWriter(client.GetStream());

                    bool rcvdNick, rcvdUser, rcvdPass;
                    rcvdPass = rcvdNick = rcvdUser = false;

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

                    _sw.WriteLine(":irc.helpmebot.org.uk 001 :Welcome to the Helpmebot IRC Gateway.");
                    foreach (var c in _baseIal.activeChannels)
                    {
                        _sw.WriteLine(":" + _baseIal.myIdentity + " JOIN " + c);
                    }



                    _sw.Flush();
                    _baseIal.dataRecievedEvent += baseIalDataRecievedEvent;

                    while (client.Connected)
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
            }
            catch (ThreadAbortException ex)
            {}
            catch (Exception ex)
            {
                GlobalFunctions.errorLog(ex);
            }
        }

        void baseIalDataRecievedEvent(string data)
        {
            _sw.WriteLine(data);
            _sw.Flush();
        }

        #region IThreadedSystem members

        public void stop()
        {
            _t.Abort();
        }

        public void registerInstance()
        {
            Threading.ThreadList.instance().register(this);
        }

        public string[] getThreadStatus()
        {
            string[] x = {_t.ThreadState.ToString()};
            return x;
        }

        public event EventHandler threadFatalError;
        #endregion
    }
}
