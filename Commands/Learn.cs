using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Learn : GenericCommand
    {
        public Learn( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "learn" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            if( args.Length >= 2 )
            {
               if( WordLearner.Learn( args[ 0 ] , string.Join( " " , args , 1 , args.Length - 1 ) ))
                   IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "cmdLearnDone" , "" ) );
               else
                   IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "cmdLearnError" , "" ) );
 
            }
            else
            {
                string[ ] messageParameters = { "learn" , "1" , args.Length.ToString( ) };
                IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );
            }
        }
    }
}
