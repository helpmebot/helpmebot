using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands.FunStuff
{
    class Trout  :GenericCommand
    {
        public Trout( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( );
        }

        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            string name = string.Join( " ", args );

            if( name.ToLower() == helpmebot6.Helpmebot6.irc.IrcNickname.ToLower() || name.ToLower() == "stwalkerster" )
            {
                name = source.Nickname;
            }

            string[ ] messageparams = { name };
            string message = IAL.wrapCTCP( "ACTION", Configuration.Singleton( ).GetMessage( "cmdTrout", messageparams ) );

            return new CommandResponseHandler( message );

        }
    }
}
