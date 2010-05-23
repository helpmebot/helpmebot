using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    class Disable: GenericCommand
    {
        public Disable( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            Monitoring.WatcherController.Instance( ).removeWatcherFromChannel( args[ 0 ] , channel );
            return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) );
        }
    }
}
