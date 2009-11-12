using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Pagewatcher : GenericCommand
    {
        public Pagewatcher( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            if( args.Length > 1 )
            {
                switch( args[0] )
                {
                    case "add":
                        addPageWatcher( args[ 1 ] , channel );
                        break;
                    case "del":
                        removePageWatcher( args[ 1 ] , channel );
                        break;
                }
            }
            return new CommandResponseHandler( );
        }

        private void addPageWatcher(string page, string channel)
        {
            throw new NotImplementedException( );
        }

        private void removePageWatcher( string page , string channel )
        {
            throw new NotImplementedException( );
        }
        
    }
}
