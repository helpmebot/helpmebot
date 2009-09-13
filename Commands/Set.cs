using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Set : GenericCommand
    {
        public Set( )
        {
            string[ ] wc = {"command = \"set\"" };
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
                    throw new ArgumentOutOfRangeException( );
                    break;
            }
        }

        protected override void execute( User source , string destination , string[ ] args )
        {

            Configuration.Singleton( ).setOption( args[ 1 ] , args[ 0 ] , args[ 2 ] );

        }
    }

}
