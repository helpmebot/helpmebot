#region Usings

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using helpmebot6.Threading;

#endregion

namespace helpmebot6.UdpListener
{
    public class UDPListener : IThreadedSystem
    {
        public UDPListener(int port)
        {
            this._key = Configuration.singleton().retrieveGlobalStringOption("udpKey");
            this._udpClient = new UdpClient(port);
            this._listenerThread = new Thread(threadMethod);
            this.registerInstance();
            this._listenerThread.Start();
        }


        private readonly Thread _listenerThread;
        private readonly UdpClient _udpClient;
        private readonly string _key;

        private void threadMethod()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            try
            {
                while (true)
                {
                    IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 0);
                    if (this._udpClient.Available != 0)
                    {
                        byte[] datagram = this._udpClient.Receive(ref ipep);

                        BinaryFormatter bf = new BinaryFormatter();
                        UdpMessage recievedMessage = (UdpMessage) bf.Deserialize(new MemoryStream(datagram));

                        byte[] bm = Encoding.ASCII.GetBytes(recievedMessage.message + this._key);
                        byte[] bh = MD5.Create().ComputeHash(bm);
                        string hash = Encoding.ASCII.GetString(bh);
                        if (hash == recievedMessage.hash)
                        {
                            Helpmebot6.irc.sendRawLine("PRIVMSG " + recievedMessage.message);
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                EventHandler temp = this.threadFatalError;
                if (temp != null)
                {
                    temp(this, new EventArgs());
                }
            }
        }

        #region IThreadedSystem Members

        public void stop()
        {
            this._listenerThread.Abort();
        }

        public void registerInstance()
        {
            ThreadList.instance().register(this);
        }

        public string[] getThreadStatus()
        {
            string[] statuses = {this._listenerThread.ThreadState.ToString()};
            return statuses;
        }

        public event EventHandler threadFatalError;

        #endregion
    }
}