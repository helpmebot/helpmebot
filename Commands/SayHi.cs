using System;
using System.Collections.Generic;
using System.Text;
using helpmebot6;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Says hi to a user
    /// </summary>
    class Sayhi : GenericCommand
    {

        public Sayhi( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "sayhi" );
        }

       protected override void execute( User toUser , string destination, string[] args )
        {
            Helpmebot6.irc.IrcPrivmsg(
                destination ,
                Configuration.Singleton().GetMessage( "cmdSayHi1" ,
                    toUser.Nickname
                    )
                );


        }
    }
}
