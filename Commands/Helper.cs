using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Helper : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            // FIXME: this needs putting into its own subsystem, messageifying, configifying, etc.

            string message = "[HELP]: " + source.ToString() + " needs help in #wikipedia-en-help !";
            if (args.Length > 0)
                message += "(\"" + string.Join(" ", args) + "\")";

            Helpmebot6.irc.IrcNotice("#wikipedia-en-helpers", message);

            return null; 
        }
    }
}
