using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace helpmebot6.Monitoring.PageWatcher
{
    class PageWatcherController
    {
        #region woo singleton
        private static PageWatcherController _instance;
        protected PageWatcherController( )
        {
            watchedPageList = new ArrayList( );
            LoadAllWatchedPages( );
            irc = new IAL( 2 );
            irc.Connect( );
        }
        public static PageWatcherController Instance( )
        {
            if( _instance == null )
                _instance = new PageWatcherController( );
            return _instance;
        }
        #endregion

        private IAL irc;

        private ArrayList watchedPageList;

        public struct RcPageChange
        {
            public string title;
            public string flags;
            public string diffUrl;
            public string user;
            public string byteDiff;
            public string comment;
        }

        private void SetupEvents( )
        {
            irc.ConnectionRegistrationSucceededEvent += new IAL.ConnectionRegistrationEventHandler( irc_ConnectionRegistrationSucceededEvent );
            irc.PrivmsgEvent += new IAL.PrivmsgEventHandler( irc_PrivmsgEvent );
        }

        private void LoadAllWatchedPages( )
        {
            throw new NotImplementedException( );
        }

        private void irc_PrivmsgEvent( User source , string destination , string message )
        {
            if( source.ToString( ) == Configuration.Singleton( ).retrieveGlobalStringOption( "wikimediaRcBot" ) )
            {
                RcPageChange rcItem = rcParser( message );
                if( watchedPageList.Contains( rcItem.title ) )
                {
                    PageWatcherNotificationEvent( rcItem );
                }
            }
        }

        private void irc_ConnectionRegistrationSucceededEvent( )
        {
            MySql.Data.MySqlClient.MySqlDataReader dr = DAL.Singleton().ExecuteReaderQuery( "SELECT `channel_name` FROM `channel` WHERE `channel_enabled` = 1 AND `channel_network` = '2';" );
            if( dr != null )
            {
                while( dr.Read( ) )
                {
                    object[ ] channel = new object[ 1 ];
                    dr.GetValues( channel );
                    irc.IrcJoin( channel[ 0 ].ToString( ) );
                }
                dr.Close( );
            }
        }

        RcPageChange rcParser( string rcItem )
        {
            return new RcPageChange( );
        }

        public void watchPage( string pageName )
        {
            // add to database

            // add to arraylist
            throw new NotImplementedException( );
        }

        public void unwatchPage( string pageName )
        {
            //remove from database

            // remove from arraylist
            throw new NotImplementedException( );
        }

        public delegate void PageWatcherNotificationEventDelegate(RcPageChange rcItem);
        public event PageWatcherNotificationEventDelegate PageWatcherNotificationEvent;


    }
}
