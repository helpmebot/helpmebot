using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Access : GenericCommand
    {
        public Access( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "access" );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            if( args.Length > 1 )
            {
                switch( args[0] )
                {
                    case "add":
                        User.userRights aL = User.userRights.Normal;

                        switch( args[2].ToLower() )
                        {
                            case "developer":
                            case "superuser":
                            case "advanced":
                            case "semi-ignored":
                            case "ignored":
                            case "normal":
                                break;
                            default:
                                break;
                        }

                        addAccessEntry( User.newFromString( args[ 1 ] ) , aL );
                        break;
                    case "del":
                        break;
                }
                // add <source> <level>

                // del <id>
            }
            return new CommandResponseHandler( );
        }

        void addAccessEntry( User newEntry , User.userRights AccessLevel )
        {
            
        }
    }
}
