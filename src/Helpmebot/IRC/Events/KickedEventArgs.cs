using System;

namespace Helpmebot.IRC.Events
{
    public class KickedEventArgs : EventArgs
    {
        public KickedEventArgs(string channel)
        {
            Channel = channel;
        }

        public string Channel { get; private set; }
    }
}