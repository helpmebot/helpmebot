using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Joins an IRC channel
    /// </summary>
    class Join : GenericCommand
    {
        public Join( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "join" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            Helpmebot6.irc.IrcJoin( args[0] );
        } 
    }
}
