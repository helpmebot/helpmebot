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

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
			string[] messageParameters = {Helpmebot6.irc.MessageCount.ToString() };
            return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "messageCountReport" , messageParameters ) );
        }
    }
}
