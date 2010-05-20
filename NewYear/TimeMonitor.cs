using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using helpmebot6.Threading;
using System.Collections;
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

            DAL.Select q = new DAL.Select(
                "tz_places",
                "ADDDATE(ADDDATE(\"" + MySqlHelper.EscapeString( targetDate ) + "\", INTERVAL -tz_offset_hours HOUR), INTERVAL -tz_offset_minutes MINUTE)"
                );
            q.setFrom( "timezones" );
            q.escapeSelects( false );
            q.addOrder( new DAL.Select.Order( "ADDDATE(ADDDATE(\"" + MySqlHelper.EscapeString( targetDate ) + "\", INTERVAL -tz_offset_hours HOUR), INTERVAL -tz_offset_minutes MINUTE)", false, false ) );
            
            ArrayList al = DAL.Singleton().executeSelect(q);

            foreach (object[] row in al)
            {


                timezoneList.Add( ( DateTime.Parse( new String( ASCIIEncoding.UTF8.GetChars( ( (byte[ ])( (object[ ])row )[ 1 ] ) ) ) ) ), (string)( row[ 0 ] ) );
            }

            RegisterInstance( );
            monitorThread.Start( );
        }

        private void monitorThreadMethod( )
        {
            try
            {
                while( timezoneList.Count > 0 )
                {
                    string places = "";
                    if( timezoneList.TryGetValue( DateTime.Parse( DateTime.Now.ToString( "yyyy-MM-dd hh:mm:ss" ) ), out places ) )
                    {
                        sendNewYearMessage( places );
                        Thread.Sleep( 1000 );
                    }
                    Thread.Sleep( 500 );
                }
            }
            catch( ThreadAbortException )
            {
                EventHandler temp = ThreadFatalError;
                if( temp != null )
                {
                    temp( this, new EventArgs( ) );
                }
            }
        }

        private void sendNewYearMessage( string places )
        {
            DAL.Select q = new DAL.Select( "channel_name" );
            q.setFrom( "channel" );
            q.addWhere( new DAL.WhereConds( "channel_enabled", "1" ) );


            foreach( object[ ] res in DAL.Singleton( ).executeSelect(q) )
                
            {
                string channel = res[ 0 ].ToString( );
                if( Configuration.Singleton( ).retrieveLocalStringOption( "newYearDateAlerting", channel ) == "true" )
                {
                    string[ ] args = { places };
                    string message = Configuration.Singleton( ).GetMessage( "newYearMessage", args );
                    Helpmebot6.irc.IrcPrivmsg( channel, message  );
                    Twitter.tweet( message );
                }
            }
        }

        #region IThreadedSystem Members

        public void Stop( )
        {
            monitorThread.Abort( );
        }

        public void RegisterInstance( )
        {
            ThreadList.instance( ).register( this );
        }

        public string[ ] getThreadStatus( )
        {
            string[ ] statuses = { this.targetDate + " " + this.monitorThread.ThreadState };
            return statuses;
        }

        public event EventHandler ThreadFatalError;
        #endregion
    }
}
