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
            accessLevel = GlobalFunctions.commandAccessLevel( "die" );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Helpmebot6.irc.IrcQuit( Configuration.Singleton( ).GetMessage( "ircQuit" , source.Nickname ) );
            return null;
        } 
    }
}
