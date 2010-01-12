using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    class Delay : GenericCommand
    {
        public Delay( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            if(args.Length>2)
            { // 2 or more args
                return Monitoring.WatcherController.Instance( ).setDelay( args[ 0 ] , int.Parse( args[ 2 ] ) );
            }
            else if( args.Length==2){
                int delay = Monitoring.WatcherController.Instance( ).getDelay( args[ 0 ] );
                string[ ] messageParams = { args[0], delay.ToString( ) };
                string message = Configuration.Singleton( ).GetMessage( "catWatcherCurrentDelay" , messageParams );
                return new CommandResponseHandler( message );
            }
            // TODO: fix
            return null;
        }
    }
}
