using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    class Status : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            string[ ] messageParams = { 
                    args[ 0 ], 
                    Monitoring.WatcherController.Instance( ).isWatcherInChannel(channel,args[0] ) 
                            ? Configuration.Singleton( ).GetMessage( "enabled" ) 
                            : Configuration.Singleton( ).GetMessage( "disabled" ), 
                    Monitoring.WatcherController.Instance( ).getDelay( args[ 0 ] ).ToString( ) 
                    };

            return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "keywordStatus", messageParams ) );
        }
    }
}