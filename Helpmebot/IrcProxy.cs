using System;

namespace helpmebot6
{
    class IrcProxy : Threading.IThreadedSystem
    {
        public IrcProxy(IAL baseIrcAccessLayer, uint port)
        {
            _base = baseIrcAccessLayer;
            _port = port;
        }

        private IAL _base;
        private uint _port;

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
