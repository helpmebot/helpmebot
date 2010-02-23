using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Fetchall:GenericCommand
    {
        public Fetchall( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            CommandResponseHandler crh = new CommandResponseHandler();
            Dictionary<string , Monitoring.CategoryWatcher>.KeyCollection kc = Monitoring.WatcherController.Instance( ).getKeywords( );
            if( GlobalFunctions.isInArray( "@cats", args ) != -1 )
            {
                GlobalFunctions.removeItemFromArray( "@cats", ref args );
                string listSep = Configuration.Singleton( ).GetMessage( "listSeparator" );
                string list = Configuration.Singleton( ).GetMessage( "allCategoryCodes" );
                foreach( string item in kc )
                {
                    list += item;
                    list += listSep;
                }

                crh.respond( list.TrimEnd( listSep.ToCharArray( ) ) );
            }
            else
            {
                foreach( string key in kc )
                {
                    crh.respond( Monitoring.WatcherController.Instance( ).forceUpdate( key ) );
                }
            }
            return crh;
        }
    }
}
