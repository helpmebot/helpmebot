using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Isgd:GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            return new CommandResponseHandler( IsGd.shorten( new Uri( args[ 0 ] ) ).ToString( ) );
        }
    }
}
