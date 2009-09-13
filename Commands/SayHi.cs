using System;
using System.Collections.Generic;
using System.Text;
using helpmebot6;

namespace helpmebot6.Commands
{
    class SayHi : GenericCommand
    {

        public SayHi( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "sayhi" );
        }

       protected override void execute( User toUser , string destination, string[] args )
        {
            IAL.singleton.IrcPrivmsg(
                destination ,
                Configuration.Singleton().GetMessage( "cmdSayHi1" ,
                    toUser.Nickname
                    )
                );


        }
    }
}
