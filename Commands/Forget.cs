using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Forget : GenericCommand
    {
        public Forget( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "forget" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            if( args.Length >= 1 )
            {
                if( WordLearner.Forget( args[ 0 ] ) )
                    IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "cmdForgetDone" , "" ) );
                else
                    IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "cmdForgetError" , "" ) );
            }
            else
            {
                string[ ] messageParameters = { "forget" , "1" , args.Length.ToString( ) };
                IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );
            }
        }
    }
}
