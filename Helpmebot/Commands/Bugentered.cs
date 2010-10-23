using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    class Bugentered : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            if (args.Count() < 2)
                return null;

            Helpmebot6.irc.ircPrivmsg( "MemoServ",
                                       "send " + args[ 0 ] +
                                       " Thank you for your bug report. You can now track this bug at http://helpmebot.org.uk/bugs/view.php?id=" +
                                       args[ 1 ] );

            return new CommandResponseHandler(
                new Message().get( "done" ),CommandResponseDestination.PrivateMessage  )
                ;
        }
    }
}
