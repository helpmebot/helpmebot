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
            string[] cols = {"watcher_category", "watcher_keyword"};
            ArrayList watchersInDb = DAL.Singleton( ).Select( cols , "watcher" , null , null , null , null , null , 100 , 0 );
            foreach( object[] item in watchersInDb )
            {
                watchers.Add( (string)item[ 1 ] , new CategoryWatcher( (string)item[ 0 ] , (string)item[ 1 ] ) );
            }
            foreach( KeyValuePair<string,CategoryWatcher> item in watchers )
            {
                
                item.Value.CategoryHasItemsEvent+=new CategoryWatcher.CategoryHasItemsEventHook(CategoryHasItemsEvent);
                item.Value.SleepTime = 60;
            }
        }

        private void addWatcher(string key, string category)
        {
            watchers.Add( key , new CategoryWatcher( category , key ) );
            CategoryWatcher cw;
            if( watchers.TryGetValue( key , out cw ) )
            {
                cw.SleepTime = 10;
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

        public bool isValidKeyword( string keyword )
        {
            return watchers.ContainsKey( keyword );
        }

        public void addWatcherToChannel( string keyword , string channel )
        {
            throw new NotImplementedException( );
        }

        public void removeWatcherFromChannel( string keyword , string channel )
        {
            throw new NotImplementedException( );
        }

        private void forceUpdate( string key )
        {
            CategoryWatcher cw;
            if( watchers.TryGetValue( key , out cw ) )
            {

            }
        }

        private void CategoryHasItemsEvent( PageList items , string keyword )
        {


            string message = compileMessage( items , keyword  );


            string[ ] queryCols = { "c.`channel_name`" };
            DAL.join[ ] queryJoin = new DAL.join[ 2 ];
            queryJoin[ 0 ].joinType = DAL.joinTypes.INNER;
            queryJoin[ 0 ].table = "channelwatchers cw";
            queryJoin[ 0 ].joinConditions = "w.`watcher_id` = cw.`cw_watcher`";
            queryJoin[ 1 ].joinType = DAL.joinTypes.INNER;
            queryJoin[ 1 ].table = "channel c";
            queryJoin[ 1 ].joinConditions = "c.`channel_id` = cw.`cw_channel`";
            string[ ] queryWhere = { "w.`watcher_keyword` = 'per'" };
            ArrayList channels = DAL.Singleton( ).Select( queryCols , "watcher w" , queryJoin , queryWhere , new string[ 0 ] , null , null , 10 , 0 );
            foreach( object[ ] item in channels )
            {
                Helpmebot6.irc.IrcPrivmsg( (string)item[ 0 ] , message );
            }
        }

        private string compileMessage( PageList items , string keyword )
        {   // keywordHasItems: 0: count, 1: plural word(s), 2: items in category
            // keywordNoItems: 0: plural word(s)
            // keywordPlural
            // keywordSingular
            string listString = "";
            foreach( Page item in items )
            {
                listString += "[[" + item.title + "]], ";
            }
            listString = listString.TrimEnd( ' ' , ',' );

            string pluralString;

            if( items.Count( ) == 1 )
            {
                pluralString = Configuration.Singleton( ).GetMessage( keyword + "Singular" , "" );
            }
            else
            {
                pluralString = Configuration.Singleton( ).GetMessage( keyword + "Plural" , "" );
            }
            string[ ] messageParams = { items.Count( ).ToString( ) , pluralString , listString };
            string message = Configuration.Singleton( ).GetMessage( keyword + "HasItems" , messageParams );
            return message;
        }


    }
}
