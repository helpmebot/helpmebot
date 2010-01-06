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
            irc = new IAL( Configuration.Singleton( ).retrieveGlobalUintOption( "wikimediaRcNetwork" ));
            SetupEvents( );
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
            this.PageWatcherNotificationEvent += new PageWatcherNotificationEventDelegate( PageWatcherController_PageWatcherNotificationEvent );
        }

        void PageWatcherController_PageWatcherNotificationEvent( PageWatcherController.RcPageChange rcItem )
        {
        }

        private void LoadAllWatchedPages( )
        {
            string[ ] sCols = { "pw_title" };
            ArrayList pL = DAL.Singleton( ).Select( sCols , "watchedpages" , null , null , null , null , null , 0 , 0 );
            foreach( object[] item in pL )
            {
                watchedPageList.Add( (string)item[ 0 ] );
            }
        }

        private void irc_PrivmsgEvent( User source , string destination , string message )
        {
            if( source.ToString( ) == Configuration.Singleton( ).retrieveGlobalStringOption( "wikimediaRcBot" ) )
            {
                RcPageChange rcItem = rcParser( message );

                // not a page edit
                if( rcItem.title == string.Empty )
                    return;

                if( watchedPageList.Contains( rcItem.title ) )
                {
                    PageWatcherNotificationEvent( rcItem );
                }
            }
        }

        private void irc_ConnectionRegistrationSucceededEvent( )
        {
            uint network = Configuration.Singleton( ).retrieveGlobalUintOption( "wikimediaRcNetwork" );
            MySql.Data.MySqlClient.MySqlDataReader dr = DAL.Singleton().ExecuteReaderQuery( "SELECT `channel_name` FROM `channel` WHERE `channel_enabled` = 1 AND `channel_network` = '"+network.ToString()+"';" );
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
            string colorCodeControlChar = "\x03";
            string[ ] colorCodes = { 
                                       colorCodeControlChar + "4" , 
                                       colorCodeControlChar + "5" , 
                                       colorCodeControlChar + "07" , 
                                       colorCodeControlChar + "10" , 
                                       colorCodeControlChar + "14" , 
                                       colorCodeControlChar + "02" , 
                                       colorCodeControlChar + "03" , 
                                       colorCodeControlChar 
                                   };

            string[ ] parts = rcItem.Split( colorCodes , StringSplitOptions.RemoveEmptyEntries );
            if( parts.Length < 12 )
            {
                return new RcPageChange( );
            }
            if( parts[ 1 ].Contains( "Special:" ) )
            {
                return new RcPageChange( );
            }

            RcPageChange ret = new RcPageChange( );
            ret.title = parts[ 1 ];
            ret.flags = parts[ 3 ].Trim( );
            ret.diffUrl = parts[ 5 ];
            ret.user = parts[ 9 ];
            ret.byteDiff = parts[ 12 ].Trim( '(' , ')' );
            if( parts.Length > 13 )
            {
                ret.comment = parts[ 13 ];
            }
            return ret;
        }

        public void watchPage( string pageName )
        {
            // add to database
            DAL.Singleton( ).ExecuteNonQuery( "insert into watchedpages values (null, \"" + pageName + "\");" );
            // add to arraylist
            watchedPageList.Add( pageName );
        }

        public void unwatchPage( string pageName )
        {
            //remove from database
            DAL.Singleton( ).ExecuteNonQuery( "delete from watchedpages where pw_title = \"" + pageName + "\";" );
            // remove from arraylist
            watchedPageList.Remove( pageName );
        }

        public delegate void PageWatcherNotificationEventDelegate(RcPageChange rcItem);
        public event PageWatcherNotificationEventDelegate PageWatcherNotificationEvent;

        public void Stop( )
        {
            irc.IrcQuit( );
        }
    }
}
