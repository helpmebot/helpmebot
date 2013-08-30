// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcProxy.cs" company="Helpmebot Development Team">
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
//   Defines the IrcProxy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    class IrcProxy : Threading.IThreadedSystem
    {
        public IrcProxy(IAL baseIrcAccessLayer, int port, string password)
        {
            if (Configuration.singleton()["enableProxy"] != "true") return;

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

                    new IrcProxyInstance(client, _password, _baseIal);
                }
            }
            catch (ThreadAbortException)
            {}
            catch (Exception ex)
            {
                GlobalFunctions.errorLog(ex);
            }
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
