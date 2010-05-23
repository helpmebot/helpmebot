using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
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
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            watchers = new Dictionary<string , CategoryWatcher>( );

            DAL.Select q = new DAL.Select( "watcher_category", "watcher_keyword", "watcher_sleeptime" );
            q.addOrder( new DAL.Select.Order( "watcher_priority", true ) );
            q.setFrom( "watcher" );
            q.addLimit( 100, 0 );
            ArrayList watchersInDb = DAL.Singleton( ).executeSelect( q );
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
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            return watchers.ContainsKey( keyword );
        }

        public bool addWatcherToChannel( string keyword , string channel )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            string channelId = Configuration.Singleton( ).getChannelId( channel );
            int watcherId = getWatcherId( keyword );

            DAL.Select q = new DAL.Select("COUNT(*)");
            q.setFrom("channelwatchers");
            q.addWhere(new DAL.WhereConds("cw_channel",channelId));
            q.addWhere(new DAL.WhereConds("cw_watcher",watcherId));
            string count = DAL.Singleton( ).executeScalarSelect( q );

            if( count == "0" )
            {
                DAL.Singleton( ).Insert( "channelwatchers", channelId, watcherId.ToString( ) );
                return true;
            }
            else
                return false;
        }

        public void removeWatcherFromChannel( string keyword , string channel )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            string channelId = Configuration.Singleton( ).getChannelId( channel );
            int watcherId = getWatcherId( keyword );

            DAL.Singleton( ).Delete( "channelwatchers", 0, new DAL.WhereConds( "cw_channel", channelId ), new DAL.WhereConds( "cw_watcher", watcherId ) );
        }

        public string forceUpdate( string key, string destination )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

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
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            ArrayList newItems = updateDatabaseTable( items, keyword );

            DAL.Select q = new DAL.Select( "channel_name" );
            q.addJoin( "channelwatchers", DAL.Select.JoinTypes.INNER, new DAL.WhereConds( false, "watcher_id", "=", false, "cw_watcher" ) );
            q.addJoin( "channel", DAL.Select.JoinTypes.INNER, new DAL.WhereConds( false, "channel_id", "=", false, "cw_channel" ) );
            q.setFrom( "watcher" );
            q.addWhere( new DAL.WhereConds( "watcher_keyword", keyword ) );
            q.addLimit( 10, 0 );

            ArrayList channels = DAL.Singleton( ).executeSelect( q );
            foreach( object[ ] item in channels )
            {
                string message = compileMessage( items, keyword, (string)item[ 0 ], false );
                Helpmebot6.irc.IrcPrivmsg( (string)item[ 0 ] , message );
            }

            if( newItems.Count > 0 )
                Twitter.tweet( compileMessage( newItems, keyword, ">TWITTER<", false ) );

        }

        private ArrayList updateDatabaseTable( ArrayList items, string keyword )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            ArrayList newItems = new ArrayList( );
            foreach( string item in items )
            {
                DAL.Select q = new DAL.Select( "COUNT(*)" );
                q.setFrom( "categoryitems" );
                q.addWhere( new DAL.WhereConds( "item_name", item ) );
                q.addWhere( new DAL.WhereConds( "item_keyword", keyword ) );
                if( DAL.Singleton( ).executeScalarSelect( q ) == "0" )
                {
                    DAL.Singleton( ).Insert( "categoryitems", "", item, "", keyword, "1" );
                    newItems.Add( item );
                }
                else
                {
                    Dictionary<string, string> v = new Dictionary<string, string>( );
                    v.Add( "item_updateflag", "1" );
                    DAL.Singleton( ).Update( "categoryitems", v, 1, new DAL.WhereConds( "item_name", item ), new DAL.WhereConds( "item_keyword", keyword ) );

                }
            }
            DAL.Singleton( ).Delete( "categoryitems", 0, new DAL.WhereConds( "item_updateflag", 0 ), new DAL.WhereConds( "item_keyword", keyword ) );
            Dictionary<string, string> val = new Dictionary<string, string>( );
            val.Add( "item_updateflag", "0" );
            DAL.Singleton( ).Update( "categoryitems", val, 0 );
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
            
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );


            string fakedestination;
            if( destination == ">TWITTER<" )
                fakedestination = "";
            else
                fakedestination = destination;

            bool showWaitTime = ( fakedestination == "" ? false : ( Configuration.Singleton( ).retrieveLocalStringOption( "showWaitTime", destination ) == "true" ? true : false ) );
            
            TimeSpan minimumWaitTime;
            if( !TimeSpan.TryParse( Configuration.Singleton( ).retrieveLocalStringOption( "minimumWaitTime", destination ), out minimumWaitTime ) )
                minimumWaitTime = new TimeSpan( 0 );

            bool shortenUrls = ( fakedestination == "" ? false : ( Configuration.Singleton( ).retrieveLocalStringOption( "useShortUrlsInsteadOfWikilinks", destination ) == "true" ? true : false ) );
            bool showDelta = ( fakedestination == "" ? false : ( Configuration.Singleton( ).retrieveLocalStringOption( "catWatcherShowDelta", destination ) == "true" ? true : false ) );

            if( destination == ">TWITTER<" )
            {
                shortenUrls = true;
                showDelta = true;
            }

            if( forceShowAll )
                showDelta = false;

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
                        listString += IsGd.shorten( new Uri( Configuration.Singleton( ).retrieveGlobalStringOption( "wikiUrl" ) + item ) ).ToString( );
                    }

                    if( showWaitTime )
                    {
                        DAL.Select q = new DAL.Select( "item_entrytime" );
                        q.addWhere( new DAL.WhereConds( "item_name", item ) );
                        q.addWhere( new DAL.WhereConds( "item_keyword", keyword ) );
                        q.setFrom( "categoryitems" );

                        string insertDate = DAL.Singleton( ).executeScalarSelect( q );
                        DateTime realInsertDate;
                        if( !DateTime.TryParse( insertDate, out realInsertDate ) )
                            realInsertDate = DateTime.Now;

                        TimeSpan ts = DateTime.Now - realInsertDate;

                        if( ts >= minimumWaitTime )
                        {
                            string[ ] messageparams = { ts.Hours.ToString( ).PadLeft( 2, '0' ), ts.Minutes.ToString( ).PadLeft( 2, '0' ), ts.Seconds.ToString( ).PadLeft( 2, '0' ) };
                            listString += Configuration.Singleton( ).GetMessage( "catWatcherWaiting", messageparams );
                        }
                    }

                    listString += ", ";
                }
                listString = listString.TrimEnd( ' ', ',' );
                string pluralString;
                if( items.Count == 1 )
                {
                    pluralString = Configuration.Singleton( ).GetMessage( keyword + "Singular", "keywordSingularDefault" );
                }
                else
                {
                    pluralString = Configuration.Singleton( ).GetMessage( keyword + "Plural", "keywordPluralDefault" );
                }
                string[ ] messageParams = { items.Count.ToString( ), pluralString, listString };
                message = Configuration.Singleton( ).GetMessage( keyword + ( showDelta ? "New" : "" ) + "HasItems", "keyword" + ( showDelta ? "New" : "" ) + "HasItemsDefault", messageParams );
            }
            else
            {
                string[ ] mp = { Configuration.Singleton( ).GetMessage( keyword + "Plural", "keywordPluralDefault" ) };
                message = Configuration.Singleton( ).GetMessage( keyword + "NoItems", "keywordNoItemsDefault", mp );
            }
            return message;
        }

        private CategoryWatcher getWatcher( string keyword )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            CategoryWatcher cw;
            bool success = watchers.TryGetValue( keyword , out cw );
            if( success )
                return cw;
            else
                return null;
        }

        public Dictionary<string,CategoryWatcher>.KeyCollection getKeywords( )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            return watchers.Keys;
        }

        private ArrayList removeBlacklistedItems( ArrayList pageList )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            DAL.Select q = new DAL.Select( "ip_title" );
            q.setFrom( "ignoredpages" );
            ArrayList blacklist = DAL.Singleton( ).executeSelect( q );

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
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            if( newDelay < 1 )
            {
                string message = Configuration.Singleton( ).GetMessage( "delayTooShort" );
                return new CommandResponseHandler( message );
            }

            CategoryWatcher cw = getWatcher( keyword );
            if( cw != null )
            {
                Dictionary<string, string> vals = new Dictionary<string, string>( );
                vals.Add( "watcher_sleeptime", newDelay.ToString( ) );
                DAL.Singleton( ).Update( "watcher", vals, 0, new DAL.WhereConds( "watcher_keyword", keyword ) );
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
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

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
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            DAL.Select q = new DAL.Select( "watcher_id" );
            q.setFrom( "watcher" );
            q.addWhere( new DAL.WhereConds( "watcher_keyword", keyword ) );
            string watcherIdString = DAL.Singleton( ).executeScalarSelect( q );

            return int.Parse( watcherIdString );
        }


        public bool isWatcherInChannel( string channel, string keyword )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            DAL.Select q =new DAL.Select("COUNT(*)");
            q.setFrom("channelwatchers");
            q.addWhere( new DAL.WhereConds( "channel_name", channel ) );
            q.addWhere( new DAL.WhereConds( "watcher_keyword", keyword ) );
            q.addJoin( "channel", DAL.Select.JoinTypes.INNER, new DAL.WhereConds( false, "cw_channel", "=", false, "channel_id" ) );
            q.addJoin( "watcher", DAL.Select.JoinTypes.INNER, new DAL.WhereConds( false, "cw_watcher", "=", false, "watcher_id" ) );

            string count = DAL.Singleton( ).executeScalarSelect( q );
            if( count == "0" )
                return false;
            else
                return true;
        }
    }
}
