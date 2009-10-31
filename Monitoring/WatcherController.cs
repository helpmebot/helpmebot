using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using DotNetWikiBot;
namespace helpmebot6.Monitoring
{
    /// <summary>
    /// Controls instances of CategoryWatchers for the bot
    /// </summary>
    class WatcherController
    {
        Dictionary<string , CategoryWatcher> watchers;

        protected WatcherController( )
        {
            watchers = new Dictionary<string , CategoryWatcher>( );
            addWatcher( "per" , "Category:Wikipedia%20protected%20edit%20requests" );
        }

        private void addWatcher(string key, string category)
        {
            watchers.Add( key , new CategoryWatcher( category , key ) );
            CategoryWatcher cw;
            if( watchers.TryGetValue( key , out cw ) )
            {
                cw.CategoryHasItemsEvent += new CategoryWatcher.CategoryHasItemsEventHook( CategoryHasItemsEvent );
            }
        }

        // woo singleton
        public static WatcherController Instance( )
        {
            if( _instance == null )
                _instance = new WatcherController( );
            return _instance;
        }
        private static WatcherController _instance;

        public void forceUpdate( string key )
        {
            CategoryWatcher cw;
            if( watchers.TryGetValue( key , out cw ) )
            {
                CategoryHasItemsEvent( cw.doCategoryCheck( ) , key );
            }
        }

        void CategoryHasItemsEvent( DotNetWikiBot.PageList items , string keyword )
        {
            foreach( Page item in items )
            {
                Helpmebot6.irc.IrcPrivmsg( "Stwalkerster" , item.ToString( ) );
            }
        }

        

    }
}
