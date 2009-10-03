using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Time : GenericCommand
    {
        public Time( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "time" ); 
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            string[ ] messageParams = { source.Nickname , DateTime.Now.DayOfWeek.ToString(),DateTime.Now.ToLongDateString( ) , DateTime.Now.ToLongTimeString( ) };
            string message = Configuration.Singleton( ).GetMessage( "cmdTime" , messageParams );
            IAL.singleton.IrcPrivmsg( destination , message );
        }
    }
}
