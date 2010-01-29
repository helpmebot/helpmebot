using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using helpmebot6.Threading;
namespace helpmebot6.NewYear
{
    class TimeMonitor : IThreadedSystem
    {
        public static TimeMonitor instance( )
        {
            if( _instance == null )
                _instance = new TimeMonitor( );

            return _instance;
        }

        private static TimeMonitor _instance;
        private Thread monitorThread;

        private string targetDate;

        private Dictionary<DateTime, string> timezoneList;

        protected TimeMonitor( )
        {
            targetDate = Configuration.Singleton( )[ "newYearDateMonitoringTarget" ];

            monitorThread = new Thread( new ThreadStart( monitorThreadMethod ) );

            timezoneList = new Dictionary<DateTime, string>( );
            string tzQuery = "SELECT tz_places as Places, ADDDATE(ADDDATE(\"" + targetDate + "\", INTERVAL -tz_offset_hours HOUR), INTERVAL -tz_offset_minutes MINUTE) as UtcDate FROM timezones t ORDER BY ADDDATE(ADDDATE(\"" + targetDate + "\", INTERVAL -tz_offset_hours HOUR), INTERVAL -tz_offset_minutes MINUTE) DESC;";

            MySqlDataReader dr = DAL.Singleton( ).ExecuteReaderQuery( tzQuery );

            while( dr.Read( ) )
            {
                object[ ] vals = new object[ 2 ];

                dr.GetValues( vals );

                System.Text.ASCIIEncoding.ASCII.GetChars( (byte[ ])vals[ 1 ] );


                timezoneList.Add( ( DateTime.Parse( new String( System.Text.ASCIIEncoding.ASCII.GetChars( (byte[ ])vals[ 1 ] ) ) ) ), vals[ 0 ].ToString( ) );
            }
            dr.Close( );


            RegisterInstance( );
            monitorThread.Start( );
        }

        private void monitorThreadMethod( )
        {

            while( timezoneList.Count > 0 )
            {
                string places = "";
                if( timezoneList.TryGetValue( DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")), out places ) )
                {
                    sendNewYearMessage( places );
                    Thread.Sleep( 1000 );
                }
                Thread.Sleep( 500 );
            }

        }

        private void sendNewYearMessage( string places )
        {
            string[ ] select = { "channel_name" };
            string[ ] wc = { "channel_enabled = 1"};

            foreach( object[] res in DAL.Singleton( ).Select( select, "channel", null, wc, null, null, null, 0, 0 ) )
            {
                string channel = res[ 0 ].ToString( );
                if( Configuration.Singleton( ).retrieveLocalStringOption( "newYearDateAlerting", channel ) == "true" )
                {
                    string[ ] args = { places };
                    Helpmebot6.irc.IrcPrivmsg( channel, Configuration.Singleton( ).GetMessage( "newYearMessage", args ) );
                }
            }
        }

        #region IThreadedSystem Members

        public void Stop( )
        {
            throw new NotImplementedException( );
        }

        public void RegisterInstance( )
        {
            ThreadList.instance( ).register( this );
        }

        public string[ ] getThreadStatus( )
        {
            throw new NotImplementedException( );
        }
        #endregion
    }
}
