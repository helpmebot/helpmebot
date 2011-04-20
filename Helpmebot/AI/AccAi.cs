using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.AI
{
    class AccAi
    {
        public static void checkCiaVcCommits(User source, string destination, string message)
        {
            if(source.nickname.Substring(0,4) == "CIA-")
                if(destination=="#wikipedia-en-accounts")
                {
                    if (message.Contains("wars:"))
                    {
                        Helpmebot6.irc.ircPrivmsg("#wikipedia-en-accounts", "!rewrite-pull");
                    }
                    if (message.Contains("wp-en-acc:"))
                    {
                        Helpmebot6.irc.ircPrivmsg("#wikipedia-en-accounts", "!sand-svnup");
                    }
                }
        }

    }
}
