using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the number of messages that have been sent by the bot to IRC
    /// </summary>
    class Messagecount : GenericCommand
    {
        public Messagecount( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "messagecount" );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "messageCountReport" , Helpmebot6.irc.MessageCount.ToString( ) ) );
        }
    }
}
