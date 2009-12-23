using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace helpmebot.NewYear
{
    class TimeMonitor
    {
        public static TimeMonitor instance( )
        {
            if( _instance == null )
                _instance = new TimeMonitor( );

            return _instance;
        }
        
        private static TimeMonitor _instance;
        private Thread monitorThread;

        protected TimeMonitor( )
        {
            monitorThread = new Thread( new ThreadStart( monitorThreadMethod ) );
            monitorThread.Start( );
        }

        private void monitorThreadMethod( )
        {
            // SELECT *, ADDDATE(ADDDATE("2010-01-01 00:00:00", INTERVAL -tz_offset_hours HOUR), INTERVAL -tz_offset_minutes MINUTE) FROM timezones t ORDER BY ADDDATE(ADDDATE("2010-01-01 00:00:00", INTERVAL -tz_offset_hours HOUR), INTERVAL -tz_offset_minutes MINUTE) DESC;

        }
    }
}
