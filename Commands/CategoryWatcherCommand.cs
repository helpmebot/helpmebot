using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class CategoryWatcher : GenericCommand
    {
        public CategoryWatcher( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            CommandResponseHandler crh = new CommandResponseHandler( );

            if( args.Length == 1 )
            { // just do category check
                crh.respond( Monitoring.WatcherController.Instance( ).forceUpdate( args[ 0 ] ) );
            }
            else
            { // do something else too.
                Type subCmdType = Type.GetType( "helpmebot6.Commands.CategoryWatcherCommand." + args[ 1 ].Substring(0,1).ToUpper( ) + args[ 1 ].Substring( 1 ).ToLower( ) );
                if( subCmdType != null )
                {
                    return ( (GenericCommand)Activator.CreateInstance( subCmdType ) ).run( source , channel , args );
                }
            }
            return crh;
        }
        
    }
}
