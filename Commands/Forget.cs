﻿using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{   
    /// <summary>
    /// Forgets a keyword
    /// </summary>
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
                    Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "cmdForgetDone" , "" ) );
                else
                    Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "cmdForgetError" , "" ) );
            }
            else
            {
                string[ ] messageParameters = { "forget" , "1" , args.Length.ToString( ) };
                Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );
            }
        }
    }
}
