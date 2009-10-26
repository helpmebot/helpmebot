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
            IAL.singleton.IrcPrivmsg( destination , Configuration.Singleton( ).GetMessage( "messageCountReport" , IAL.singleton.MessageCount.ToString( ) ) );
        }
    }
}
