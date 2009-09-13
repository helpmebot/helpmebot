using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Join : GenericCommand
    {
        public Join( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "join" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            IAL.singleton.IrcJoin( args[0] );
        } 
    }
}
