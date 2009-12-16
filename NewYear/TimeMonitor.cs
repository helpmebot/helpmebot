using System;
using System.Collections.Generic;
using System.Text;

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

        protected TimeMonitor( )
        {

        }
    }
}
