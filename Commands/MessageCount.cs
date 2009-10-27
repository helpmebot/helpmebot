using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Messagecount : GenericCommand
    {
        public Messagecount( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "messagecount" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            Helpmebot6.irc.IrcPrivmsg( destination , Configuration.Singleton( ).GetMessage( "messageCountReport" , Helpmebot6.irc.MessageCount.ToString( ) ) );
        }
    }
}
