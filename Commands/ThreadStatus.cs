using System;
using System.Collections.Generic;
using System.Text;
using helpmebot6.Threading;

namespace helpmebot6.Commands
{
    class ThreadStatus : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            string[ ] statuses = ThreadList.instance( ).getAllThreadStatus( );
            CommandResponseHandler crh = new CommandResponseHandler( );
            foreach( string item in statuses )
            {
                crh.respond( item );
            }
            return crh;
        }
    }
}
