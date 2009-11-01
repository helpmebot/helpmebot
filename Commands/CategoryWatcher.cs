using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class CategoryWatcher : GenericCommand
    {
        public CategoryWatcher( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "categorywatcher" );
        }

        protected override CommandResponseHandler execute( User source , string[ ] args )
        {
            CommandResponseHandler crh = new CommandResponseHandler( );

            if( args.Length == 1 )
            { // just do category check
                crh.respond( Monitoring.WatcherController.Instance( ).forceUpdate( args[ 0 ] ) );
            }
            else
            { // do something else too.

            }
            return crh;
        }
        
    }
}
