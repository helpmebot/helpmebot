using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
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

            string forbiddenTargets = { "stwalkerster", "itself", "himself", "herself", Helpmebot6.irc.IrcNickname.ToLower( ) };

            if( GlobalFunctions.isInArray(name,forbiddenTargets) != -1)
            {
                name = source.Nickname;
            }

            string[ ] messageparams = { name };
            string message = IAL.wrapCTCP( "ACTION", Configuration.Singleton( ).GetMessage( "cmdTrout", messageparams ) );

            return new CommandResponseHandler( message );

        }
    }
}
