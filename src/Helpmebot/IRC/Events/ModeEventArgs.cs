using System.Collections.Generic;
using Helpmebot.IRC.Messages;
using Helpmebot.Model.Interfaces;

namespace Helpmebot.IRC.Events
{
    public class ModeEventArgs : UserEventArgsBase
    {
        public string Target { get; private set; }
        public IEnumerable<string> Changes { get; private set; }

        public ModeEventArgs(IMessage message, IUser user, string target, IEnumerable<string> changes) : base(message, user)
        {
            Target = target;
            Changes = changes;
        }
    }
}