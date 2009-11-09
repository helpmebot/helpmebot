using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    class Delay : GenericCommand
    {
        public Delay( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "CategoryWatcherCommand.Delay" );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            if(args.Length>1)
            { // 2 or more args
                Monitoring.WatcherController.Instance( ).setDelay( args[ 0 ] , int.Parse( args[ 1 ] ) );
            }
            else if( args.Length==1){
                int delay = Monitoring.WatcherController.Instance( ).getDelay( args[ 0 ] );
            }
            // TODO: fix
            return null;
        }
    }
}
