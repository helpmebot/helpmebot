using System;
using System.Collections.Generic;
using System.Text;
using helpmebot6;

namespace helpmebot6.Commands
{
    class SayHi
    {
        public static void execute( string toUser , string destination )
        {
            IAL.singleton.IrcPrivmsg(
                destination ,
                Configuration.singleton.GetMessage( "cmdSayHi1" ,
                    toUser
                    )
                );


        }
    }
}
