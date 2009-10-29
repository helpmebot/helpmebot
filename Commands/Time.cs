using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the current date/time
    /// </summary>
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
            Helpmebot6.irc.IrcPrivmsg( destination , message );
        }
    }

    /// <summary>
    /// Returns the current date/time. Alias for Time.
    /// </summary>
    class Date : Time
    {

    }
}
