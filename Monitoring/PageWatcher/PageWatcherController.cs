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
            if( Helpmebot6.pagewatcherEnabled )
            {
                LoadAllWatchedPages( );
                uint wikiRCIrc = Configuration.Singleton( ).retrieveGlobalUintOption( "wikimediaRcNetwork" );
                if( wikiRCIrc != 0 )
                {
                    irc = new IAL( wikiRCIrc );
                    SetupEvents( );
                    irc.Connect( );
                }
            }
            
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

        public string[ ] getWatchedPages( )
        {
            string[ ] wp = new string[ watchedPageList.Count ];
            watchedPageList.CopyTo( wp );
            return wp;
        }

        public void LoadAllWatchedPages( )
        {
            watchedPageList.Clear( );
            DAL.Select q = new DAL.Select( "pw_title" );
            q.setFrom( "watchedpages" );
            ArrayList pL = DAL.Singleton( ).executeSelect( q );
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

            DAL.Select q = new DAL.Select( "channel_name" );
            q.setFrom( "channel" );
            q.addWhere( new DAL.WhereConds( "channel_enabled", 1 ) );
            q.addWhere( new DAL.WhereConds( "channel_network", network.ToString( ) ) );
            foreach( object[] item in DAL.Singleton( ).executeSelect( q ) )
            {
                irc.IrcJoin( (string)( item[0] ) );

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
            // addOrder to database
            DAL.Singleton( ).Insert( "watchedpages", "", pageName );
            // addOrder to arraylist
            watchedPageList.Add( pageName );
        }

        public void unwatchPage( string pageName )
        {
            //remove from database
            DAL.Singleton( ).Delete( "watchedpages", 0, new DAL.WhereConds( "pw_title", pageName ) );
            // remove from arraylist
            watchedPageList.Remove( pageName );
        }

        public delegate void PageWatcherNotificationEventDelegate(RcPageChange rcItem);
        public event PageWatcherNotificationEventDelegate PageWatcherNotificationEvent;

    }
}
