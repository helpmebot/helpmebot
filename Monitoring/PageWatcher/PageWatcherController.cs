using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Monitoring.PageWatcher
{
    class PageWatcherController
    {
        #region woo singleton
        private static PageWatcherController _instance;
        protected PageWatcherController( )
        {
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

        private void SetupEvents( )
        {
            irc.ConnectionRegistrationSucceededEvent += new IAL.ConnectionRegistrationEventHandler( irc_ConnectionRegistrationSucceededEvent );
            irc.PrivmsgEvent += new IAL.PrivmsgEventHandler( irc_PrivmsgEvent );
        }

        void irc_PrivmsgEvent( User source , string destination , string message )
        {
            throw new NotImplementedException( );
        }

        void irc_ConnectionRegistrationSucceededEvent( )
        {
            MySql.Data.MySqlClient.MySqlDataReader dr = dbal.ExecuteReaderQuery( "SELECT `channel_name` FROM `channel` WHERE `channel_enabled` = 1 AND `channel_network` = '2';" );
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

    }
}
