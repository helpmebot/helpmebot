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
                WordLearner.Learn( args[ 0 ] , string.Join( " " , args , 1 , args.Length - 1 ) );
                IAL.singleton.IrcNotice( source.Nickname , "Done." );
            }
            else
            {
                IAL.singleton.IrcNotice( source.Nickname , "Not enough arguments." );
            }
        }
    }
}
