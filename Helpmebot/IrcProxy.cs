using System;
using System.Net.Sockets;

namespace helpmebot6
{
    class IrcProxy : Threading.IThreadedSystem
    {
        public IrcProxy(IAL baseIrcAccessLayer, uint port)
        {
            _base = baseIrcAccessLayer;
            _port = port;

            _base.dataRecievedEvent += new IAL.DataRecievedEventHandler(_base_dataRecievedEvent);
        }


        private TcpListener listener;

        private IAL _base;
        private uint _port;
        





        void _base_dataRecievedEvent(string data)
        {
            throw new NotImplementedException();
        }



        #region IThreadedSystem members

        public void stop()
        {
            throw new NotImplementedException();
        }

        public void registerInstance()
        {
            throw new NotImplementedException();
        }

        public string[] getThreadStatus()
        {
            throw new NotImplementedException();
        }

        public event EventHandler threadFatalError;
        #endregion
    }
}
