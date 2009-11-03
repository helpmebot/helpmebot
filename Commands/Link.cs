using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Link : GenericCommand
    {
        public Link( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "link" );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            string key = channel;
            if( GlobalFunctions.RealArrayLength(args) > 0 )
            {
                key = "<<<REALTIME>>>";
                Linker.Instance( ).ParseMessage( string.Join( " " , args ) , key );
            }

            return new CommandResponseHandler( Linker.Instance( ).GetLink( key ) );
        }
    }
}
