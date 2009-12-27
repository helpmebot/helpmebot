using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Kills the bot.
    /// </summary>
    class Die : GenericCommand
    {
        public Die( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            string[] messageParams = { source.Nickname };
            Helpmebot6.irc.IrcQuit( Configuration.Singleton( ).GetMessage( "ircQuit" , messageParams ) );
            return null;
        } 
    }
}
