using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using helpmebot6;
using helpmebot6.Commands;

namespace helpmebot6.Commands
{
    internal class NotifyOnJoin : GenericCommand
    {
        private static object dictlock = new object();
        private static Dictionary<User, List<User>> RequestedNotifications = new Dictionary<User, List<User>>();

        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            if (args.Length != 1) return new CommandResponseHandler("I actually expected a nickname as single argument");
            User trigger;
            lock (dictlock)
            {
                User toNotify = source;
                trigger = User.newFromString(args[0]);
                if (!RequestedNotifications.ContainsKey(trigger)) RequestedNotifications.Add(trigger,new List<User>());
                RequestedNotifications[trigger].Add(toNotify);
            }
            return new CommandResponseHandler(String.Format("I'll send you a private message when someone with nickname {0} joins a channel I'm in", trigger.nickname));
        }

        internal void notifyJoin(User source, string channel)
        {
            List<User> toNotify = null;
            lock (dictlock)
            {
                if (RequestedNotifications.TryGetValue(source, out toNotify))
                {
                    RequestedNotifications.Remove(source);
                }
            }
            if (toNotify != null)
            {
                string message = String.Format("I just saw {0} enter channel {1}", source.nickname, channel);
                foreach (User user in toNotify)
                {
                    Helpmebot6.irc.ircPrivmsg(user.nickname, message);
                }
            }
        }
    }
}
