using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.AI
{
    public class AccAi
    {

        public static bool activepull = false;

        public static void checkCiaVcCommits(User source, string destination, string message)
        {
            if (source.nickname.Length >= 5)
                if (source.nickname.Substring(0, 4) == "CIA-")
                    if (destination == "#wikipedia-en-accounts-devs")
                    {
                        if (message.Contains("wars:"))
                            if (message.Contains("master"))
                            {
                                if (!activepull)
                                {
                                    Helpmebot6.irc.ircPrivmsg("#wikipedia-en-accounts-devs", "!rewrite-pull");
                                    activepull = true;
                                }
                            }
                        if (message.Contains("wp-en-acc:"))
                        {
                            Helpmebot6.irc.ircPrivmsg("#wikipedia-en-accounts-devs", "!sand-svnup");
                        }
                    }
            if (source.nickname == "ACCBot")
                if (destination == "#wikipedia-en-accounts-devs")
                {
                    if (message.Contains("http://toolserver.org/~acc/rewrite/acc.php"))
                        activepull = false;
                }

        }

    }
}

