using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    class Enable : GenericCommand
    {
        public Enable( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            if( Monitoring.WatcherController.Instance( ).addWatcherToChannel( args[ 0 ], channel ) )
                return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) );
            return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "no-change" ) );
        }
    }
}
