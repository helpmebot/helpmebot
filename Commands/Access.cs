using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Access : GenericCommand
    {
        public Access( )
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
                        if( args.Length > 2 )
                        {
                            User.userRights aL = User.userRights.Normal;

                            switch( args[ 2 ].ToLower( ) )
                            {
                                case "developer":
                                    aL = User.userRights.Developer;
                                    break;
                                case "superuser":
                                    aL = User.userRights.Superuser;
                                    break;
                                case "advanced":
                                    aL = User.userRights.Advanced;
                                    break;
                                case "semi-ignored":
                                    aL = User.userRights.Semiignored;
                                    break;
                                case "ignored":
                                    aL = User.userRights.Ignored;
                                    break;
                                case "normal":
                                    aL = User.userRights.Normal;
                                    break;
                                default:
                                    break;
                            }

                            addAccessEntry( User.newFromString( args[ 1 ] ) , aL );
                        }
                        break;
                    case "del":
                        delAccessEntry( int.Parse( args[ 1 ] ) );
                        break;
                }
                // add <source> <level>

                // del <id>
            }
            return new CommandResponseHandler( );
        }

        void addAccessEntry( User newEntry , User.userRights AccessLevel )
        {
            Logger.Instance( ).addToLog( "Adding access entry for " + newEntry.ToString( ) + " at level " + AccessLevel.ToString( ) , Logger.LogTypes.COMMAND );
            DAL.Singleton( ).ExecuteNonQuery( "INSERT INTO `user` VALUES(NULL, '" + newEntry.Nickname + "', '" + newEntry.Username + "', '" + newEntry.Hostname + "', '" + AccessLevel.ToString( ) + "');" );
        }

        void delAccessEntry( int id )
        {
            Logger.Instance( ).addToLog( "Removing access entry #" + id.ToString( ) , Logger.LogTypes.COMMAND );
            DAL.Singleton( ).ExecuteNonQuery( "DELETE FROM `user` WHERE `user_id` = " + id.ToString( ) + " LIMIT 1;" );
        }
    }
}
