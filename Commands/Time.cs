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
 
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            string[ ] messageParams = { source.Nickname , DateTime.Now.DayOfWeek.ToString(),DateTime.Now.ToLongDateString( ) , DateTime.Now.ToLongTimeString( ) };
            string message = Configuration.Singleton( ).GetMessage( "cmdTime" , messageParams );
            return new CommandResponseHandler( message );
        }
    }

    /// <summary>
    /// Returns the current date/time. Alias for Time.
    /// </summary>
    class Date : Time
    {

    }
}
