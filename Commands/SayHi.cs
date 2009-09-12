using System;
using System.Collections.Generic;
using System.Text;
using helpmebot6;

namespace helpmebot6.Commands
{
    class SayHi
    {
        static string execute()
        {
            return Configuration.singleton.GetMessage( "cmdSayHi1" ,
                    toUser
                    );

        }
    }
}
