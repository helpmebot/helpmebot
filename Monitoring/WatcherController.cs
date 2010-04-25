﻿using System;
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
            string[] cols = {"watcher_category", "watcher_keyword", "watcher_sleeptime"};
            DAL.order[ ] o = new DAL.order[ 1 ];
            o[ 0 ].asc = true;
            o[ 0 ].column = "watcher_priority";
            ArrayList watchersInDb = DAL.Singleton( ).Select( cols , "watcher" , null ,null , null , o , null , 100 , 0 );
            foreach( object[] item in watchersInDb )
            {
                watchers.Add( (string)item[ 1 ] , new CategoryWatcher( (string)item[ 0 ] , (string)item[ 1 ] , int.Parse( ( (UInt32)item[ 2 ] ).ToString( ) ) ) );
            }
            foreach( KeyValuePair<string,CategoryWatcher> item in watchers )
            {
                item.Value.CategoryHasItemsEvent+=new CategoryWatcher.CategoryHasItemsEventHook(CategoryHasItemsEvent);
            }
        }

        /*private void addWatcher(string key, string category)
        {
            watchers.Add( key , new CategoryWatcher( category , key ) );
            CategoryWatcher cw;
            if( watchers.TryGetValue( key , out cw ) )
            {
                cw.SleepTime = 10;
                cw.CategoryHasItemsEvent += new CategoryWatcher.CategoryHasItemsEventHook( CategoryHasItemsEvent );
            }
        }*/

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

        public bool addWatcherToChannel( string keyword , string channel )
        {
            string channelId = Configuration.Singleton( ).getChannelId( channel );
            int watcherId = getWatcherId( keyword );

            string[ ] wc = { "cw_channel = " + channelId, "cw_watcher = " + watcherId };
            string count = DAL.Singleton( ).Select( "COUNT(*)", "channelwatchers", null, wc, null, null, null, 0, 0 );

            if( count == "0" )
            {
                DAL.Singleton( ).ExecuteNonQuery( "INSERT INTO channelwatchers VALUES ( " + channelId + " , " + watcherId.ToString( ) + " );" );
                return true;
            }
            else
                return false;
        }

        public void removeWatcherFromChannel( string keyword , string channel )
        {
            string channelId = Configuration.Singleton( ).getChannelId( channel );
            int watcherId = getWatcherId( keyword );

            DAL.Singleton( ).ExecuteNonQuery( "DELETE FROM channelwatchers WHERE cw_channel = " + channelId + " AND cw_watcher = " + watcherId.ToString( ) + " ;" );
        }

        public string forceUpdate( string key, string destination )
        {
            CategoryWatcher cw;
            if( watchers.TryGetValue( key , out cw ) )
            {
                ArrayList items = cw.doCategoryCheck( );
                updateDatabaseTable( items, key );
                return compileMessage( items , key, destination, true );
            }
            else
                return null;
        }

        private void CategoryHasItemsEvent( ArrayList items , string keyword )
        {
            ArrayList newItems = updateDatabaseTable( items, keyword );

            string[ ] queryCols = { "c.`channel_name`" };
            DAL.join[ ] queryJoin = new DAL.join[ 2 ];
            queryJoin[ 0 ].joinType = DAL.joinTypes.INNER;
            queryJoin[ 0 ].table = "channelwatchers cw";
            queryJoin[ 0 ].joinConditions = "w.`watcher_id` = cw.`cw_watcher`";
            queryJoin[ 1 ].joinType = DAL.joinTypes.INNER;
            queryJoin[ 1 ].table = "channel c";
            queryJoin[ 1 ].joinConditions = "c.`channel_id` = cw.`cw_channel`";
            string[ ] queryWhere = { "w.`watcher_keyword` = '"+keyword+"'" };
            ArrayList channels = DAL.Singleton( ).Select( queryCols , "watcher w" , queryJoin , queryWhere , new string[ 0 ] , null , null , 10 , 0 );
            foreach( object[ ] item in channels )
            {
                string message = compileMessage( items, keyword, (string)item[ 0 ], false );
                Helpmebot6.irc.IrcPrivmsg( (string)item[ 0 ] , message );
            }

            Twitter.tweet( compileMessage( newItems, keyword, ">TWITTER<", false ) );

        }

        private ArrayList updateDatabaseTable( ArrayList items, string keyword )
        {
            ArrayList newItems = new ArrayList( );
            foreach( string item in items )
            {
                string[ ] wc = { "item_name = '" + item + "'", "item_keyword = '" + keyword + "'" };
                if( DAL.Singleton( ).Select( "COUNT(*)", "categoryitems", new DAL.join[ 0 ], wc, null, null, null, 0, 0 ) == "0" )
                {
                    DAL.Singleton( ).ExecuteNonQuery( "INSERT INTO categoryitems VALUES( null, '" + item + "', null, '" + keyword + "', 1);" );
                    newItems.Add( item );
                }
                else
                {
                    DAL.Singleton( ).ExecuteNonQuery( "UPDATE categoryitems SET item_updateflag = 1 WHERE item_name = '" + item + "' AND item_keyword = '" + keyword + "' LIMIT 1;" );
                }
            }
            DAL.Singleton( ).ExecuteNonQuery( "DELETE FROM categoryitems WHERE item_updateflag = 0 AND item_keyword = '" + keyword + "';" );
            DAL.Singleton( ).ExecuteNonQuery( "UPDATE categoryitems SET item_updateflag = 0;" );
            return newItems;
        }

        //private string compileMessage( ArrayList items, string keyword )
        //{
        //    return compileMessage( items, keyword, "" , false);
        //}
        private string compileMessage( ArrayList items, string keyword, string destination, bool forceShowAll )
        {   // keywordHasItems: 0: count, 1: plural word(s), 2: items in category
            // keywordNoItems: 0: plural word(s)
            // keywordPlural
            // keywordSingular

            string fakedestination;
            if( destination == ">TWITTER<" )
                fakedestination = "";
            else
                fakedestination = destination;

            bool showWaitTime = ( fakedestination == "" ? false : ( Configuration.Singleton( ).retrieveLocalStringOption( "showWaitTime", destination ) == "true" ? true : false ) );
            bool shortenUrls = ( fakedestination == "" ? false : ( Configuration.Singleton( ).retrieveLocalStringOption( "useShortUrlsInsteadOfWikilinks", destination ) == "true" ? true : false ) );
            bool showDelta = ( fakedestination == "" ? false : ( Configuration.Singleton( ).retrieveLocalStringOption( "catWatcherShowDelta", destination ) == "true" ? true : false ) );

            if( destination == ">TWITTER<" )
            {
                shortenUrls = true;
                showDelta = true;
            }

            if( forceShowAll )
                showDelta = false;

            items = removeBlacklistedItems( items );

            string message;

            if( items.Count > 0 )
            {
                string listString = "";
                foreach( string item in items )
                {
                    if( !shortenUrls )
                    {
                        listString += "[[" + item + "]]";
                    }
                    else
                    {
                        listString += IsGd.shorten( new Uri( Configuration.Singleton( ).retrieveGlobalStringOption( "wikiUrl" ) + item ) ).ToString();
                    }

                    if( showWaitTime )
                    {
                        string[ ] wc = { "item_name = '" + item + "'", "item_keyword = '" + keyword + "'" };
                        TimeSpan ts = DateTime.Now - DateTime.Parse( DAL.Singleton( ).Select( "item_entrytime", "categoryitems", null, wc, null, null, null, 1, 0 ) );
                        string[ ] messageparams = { ts.Hours.ToString( ).PadLeft( 2, '0' ), ts.Minutes.ToString( ).PadLeft( 2, '0' ), ts.Seconds.ToString( ).PadLeft( 2, '0' ) };
                        listString += Configuration.Singleton( ).GetMessage( "catWatcherWaiting", messageparams );
                    }

                    listString += ", ";
                }
                listString = listString.TrimEnd( ' ' , ',' );
                string pluralString;
                if( items.Count == 1 )
                {
                    pluralString = Configuration.Singleton( ).GetMessage( keyword + "Singular" , "keywordSingularDefault" );
                }
                else
                {
                    pluralString = Configuration.Singleton( ).GetMessage( keyword + "Plural" , "keywordPluralDefault" );
                }
                string[ ] messageParams = { items.Count.ToString( ) , pluralString , listString };
                message = Configuration.Singleton( ).GetMessage( keyword + ( showDelta ? "New" : "" ) + "HasItems", "keyword" + ( showDelta ? "New" : "" ) + "HasItemsDefault", messageParams );
            }
            else
            {
                message = Configuration.Singleton( ).GetMessage( keyword + "NoItems" , "keywordNoItemsDefault" , Configuration.Singleton( ).GetMessage( keyword + "Plural" , "keywordPluralDefault" ) );
            }
            return message;
        }

        private CategoryWatcher getWatcher( string keyword )
        {
            CategoryWatcher cw;
            bool success = watchers.TryGetValue( keyword , out cw );
            if( success )
                return cw;
            else
                return null;
        }

        public Dictionary<string,CategoryWatcher>.KeyCollection getKeywords( )
        {
            return watchers.Keys;
        }

        private ArrayList removeBlacklistedItems( ArrayList pageList )
        {
            string[ ] cols = { "ip_title" };
            ArrayList blacklist = DAL.Singleton( ).Select( cols , "ignoredpages" , null , null , null , null , null , 0 , 0 );

            foreach( object[] item in blacklist )
            {
                if( pageList.Contains( (string)item[ 0 ] ) )
                {
                    pageList.Remove( (string)item[ 0 ] );
                }
            }

            return pageList;
        }

        public CommandResponseHandler setDelay( string keyword , int newDelay )
        {
            if( newDelay < 1 )
            {
                string message = Configuration.Singleton( ).GetMessage( "delayTooShort" );
                return new CommandResponseHandler( message );
            }

            CategoryWatcher cw = getWatcher( keyword );
            if( cw != null )
            {
                DAL.Singleton( ).ExecuteNonQuery( "UPDATE watcher SET watcher_sleeptime = " + newDelay + " WHERE watcher_keyword = '" + keyword + "';" );
                cw.SleepTime = newDelay;
                return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) );
            }
            else
            {
                return new CommandResponseHandler( );
            }
        }

        public int getDelay( string keyword )
        {
            CategoryWatcher cw = getWatcher( keyword );
            if( cw != null )
            {
                return cw.SleepTime;
            }
            else
                return 0;
        }

        private int getWatcherId( string keyword )
        {
            string[ ] wC = { "w.`watcher_keyword` = '" + keyword + "'" };
            string watcherIdString = DAL.Singleton( ).Select( "w.`watcher_id`" , "watcher w" , null , wC , null , null , null , 1 , 0 );

            return int.Parse( watcherIdString );
        }


        public bool isWatcherInChannel( string channel, string keyword )
        {
            DAL.join[ ] j = new DAL.join[ 2 ];
            j[ 0 ].table = "channel";
            j[ 0 ].joinType = DAL.joinTypes.INNER;
            j[ 0 ].joinConditions = "cw_channel = channel_id";
            j[ 1 ].joinConditions = "cw_watcher = watcher_id";
            j[ 1 ].joinType = DAL.joinTypes.INNER;
            j[ 1 ].table = "watcher";
            string[ ] w = new string[ 2 ];
            w[ 0 ] = "channel_name = \"" + channel + "\"";
            w[ 1 ] = "watcher_keyword = \"" + keyword + "\"";
            string count = DAL.Singleton( ).Select( "COUNT(*)", "channelwatchers", j, w, null, null, null, 0, 0 );
            if( count == "0" )
                return false;
            else
                return true;
        }
    }
}
