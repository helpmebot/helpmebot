using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Rawctcp : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            string cmd = GlobalFunctions.popFromFront( ref args );

            Helpmebot6.irc.SendRawLine( IAL.wrapCTCP( cmd, string.Join( " ", args ) ) );

            return null;
        }
    }
}
