using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Forget : GenericCommand
    {
        public Forget( )
        {
            string[ ] wc = { "command = \"forget\"" };
            string al = DAL.Singleton( ).Select( "accesslevel" , "command" , null , wc , null , null , null , 1 , 0 );
            switch( al )
            {
                case "Superuser":
                    accessLevel = User.userRights.Superuser;
                    break;
                case "Advanced":
                    accessLevel = User.userRights.Advanced;
                    break;
                case "Normal":
                    accessLevel = User.userRights.Normal;
                    break;
                case "Semi-ignored":
                    accessLevel = User.userRights.Semiignored;
                    break;
                case "Ignored":
                    accessLevel = User.userRights.Ignored;
                    break;
                default:
                    throw new ArgumentOutOfRangeException( "Command not in commandlist" );
                    break;
            }
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            if( args.Length >= 1 )
            {
                WordLearner.Forget( args[ 0 ] );
                IAL.singleton.IrcNotice( source.Nickname , "Done." );
            }
            else
            {
                IAL.singleton.IrcNotice( source.Nickname , "Not enough arguments." );
            }
        }
    }
}
