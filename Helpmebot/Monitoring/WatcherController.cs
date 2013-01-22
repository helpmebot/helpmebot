// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MySql.Data.MySqlClient;

#endregion

namespace helpmebot6.Monitoring
{
    /// <summary>
    ///   Controls instances of CategoryWatchers for the bot
    /// </summary>
    internal class WatcherController
    {
        private readonly Dictionary<string, CategoryWatcher> _watchers;

        /// <summary>
        /// Initializes a new instance of the <see cref="WatcherController"/> class.
        /// </summary>
        protected WatcherController()
        {
            this._watchers = new Dictionary<string, CategoryWatcher>();

            DAL.Select q = new DAL.Select("watcher_category", "watcher_keyword", "watcher_sleeptime");
            q.addOrder(new DAL.Select.Order("watcher_priority", true));
            q.setFrom("watcher");
            q.addLimit(100, 0);
            ArrayList watchersInDb = DAL.singleton().executeSelect(q);
            foreach (object[] item in watchersInDb)
            {
                this._watchers.Add((string) item[1],
                             new CategoryWatcher((string) item[0], (string) item[1],
                                                 int.Parse(((UInt32) item[2]).ToString())));
            }
            foreach (KeyValuePair<string, CategoryWatcher> item in this._watchers)
            {
                item.Value.categoryHasItemsEvent += categoryHasItemsEvent;
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
        public static WatcherController instance()
        {
            return _instance ?? ( _instance = new WatcherController( ) );
        }

        private static WatcherController _instance;

        /// <summary>
        /// Determines whether the specified word is a valid keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        /// 	<c>true</c> if the specified word is a valid keyword; otherwise, <c>false</c>.
        /// </returns>
        public bool isValidKeyword(string keyword)
        {
            return this._watchers.ContainsKey(keyword);
        }

        /// <summary>
        /// Adds the watcher to channel.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        public bool addWatcherToChannel(string keyword, string channel)
        {
            string channelId = Configuration.singleton().getChannelId(channel);
            int watcherId = getWatcherId(keyword);

            DAL.Select q = new DAL.Select("COUNT(*)");
            q.setFrom("channelwatchers");
            q.addWhere(new DAL.WhereConds("cw_channel", channelId));
            q.addWhere(new DAL.WhereConds("cw_watcher", watcherId));
            string count = DAL.singleton().executeScalarSelect(q);

            if (count == "0")
            {
                DAL.singleton().insert("channelwatchers", channelId, watcherId.ToString());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the watcher from channel.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="channel">The channel.</param>
        public void removeWatcherFromChannel(string keyword, string channel)
        {
            string channelId = Configuration.singleton().getChannelId(channel);
            int watcherId = getWatcherId(keyword);

            DAL.singleton().delete("channelwatchers", 0, new DAL.WhereConds("cw_channel", channelId),
                                   new DAL.WhereConds("cw_watcher", watcherId));
        }

        /// <summary>
        /// Forces the update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public string forceUpdate(string key, string destination)
        {
            CategoryWatcher cw;
            if (this._watchers.TryGetValue(key, out cw))
            {
                ArrayList items = cw.doCategoryCheck();
                updateDatabaseTable(items, key);
                return compileMessage(items, key, destination, true);
            }
            return null;
        }

        private static void categoryHasItemsEvent(ArrayList items, string keyword)
        {
            ArrayList newItems = updateDatabaseTable(items, keyword);

            DAL.Select q = new DAL.Select("channel_name");
            q.addJoin("channelwatchers", DAL.Select.JoinTypes.Inner,
                      new DAL.WhereConds(false, "watcher_id", "=", false, "cw_watcher"));
            q.addJoin("channel", DAL.Select.JoinTypes.Inner,
                      new DAL.WhereConds(false, "channel_id", "=", false, "cw_channel"));
            q.setFrom("watcher");
            q.addWhere(new DAL.WhereConds("watcher_keyword", keyword));
            q.addLimit(10, 0);

            ArrayList channels = DAL.singleton().executeSelect(q);
            foreach (object[] item in channels)
            {
                string channel = (string) item[0];

                string message = compileMessage(items, keyword, channel, false);
                if (Configuration.singleton()["silence",channel] == "false")
                    Helpmebot6.irc.ircPrivmsg(channel, message);
            }

            if (newItems.Count > 0)
            {
                var message = compileMessage(newItems, keyword, ">TWITTER<", false);
       
                    new Twitter().updateStatus(message);
              
            }
        }

        private static ArrayList updateDatabaseTable(ArrayList items, string keyword)
        {
            ArrayList newItems = new ArrayList();
            foreach (string item in items)
            {
                DAL.Select q = new DAL.Select("COUNT(*)");
                q.setFrom("categoryitems");
                q.addWhere(new DAL.WhereConds("item_name", item));
                q.addWhere(new DAL.WhereConds("item_keyword", keyword));

                string dbResult;
                try
                {
                    dbResult = DAL.singleton().executeScalarSelect(q);
                }
                catch(MySqlException ex)
                {
                    GlobalFunctions.errorLog(ex);
                    dbResult = "0";
                }

                if (dbResult == "0")
                {
                    DAL.singleton().insert("categoryitems", "", item, "", keyword, "1");
                    newItems.Add(item);
                }
                else
                {
                    Dictionary<string, string> v = new Dictionary<string, string>
                                                       {
                                                           {
                                                               "item_updateflag",
                                                               "1"
                                                               }
                                                       };
                    DAL.singleton().update("categoryitems", v, 1, new DAL.WhereConds("item_name", item),
                                           new DAL.WhereConds("item_keyword", keyword));
                }
            }
            DAL.singleton().delete("categoryitems", 0, new DAL.WhereConds("item_updateflag", 0),
                                   new DAL.WhereConds("item_keyword", keyword));
            Dictionary<string, string> val = new Dictionary<string, string>
                                                 { { "item_updateflag", "0" } };
            DAL.singleton().update("categoryitems", val, 0);
            return newItems;
        }

        //private string compileMessage( ArrayList items, string keyword )
        //{
        //    return compileMessage( items, keyword, "" , false);
        //}
        private static string compileMessage(ArrayList items, string keyword, string destination, bool forceShowAll)
        {
            // keywordHasItems: 0: count, 1: plural word(s), 2: items in category
            // keywordNoItems: 0: plural word(s)
            // keywordPlural
            // keywordSingular           

            string fakedestination = destination == ">TWITTER<" ? "" : destination;

            bool showWaitTime = (fakedestination != "" && (Configuration.singleton()["showWaitTime",destination] == "true"));

            TimeSpan minimumWaitTime;
            if (
                !TimeSpan.TryParse(Configuration.singleton()["minimumWaitTime",destination],
                                   out minimumWaitTime))
                minimumWaitTime = new TimeSpan(0);

            bool shortenUrls = (fakedestination != "" && (Configuration.singleton()["useShortUrlsInsteadOfWikilinks", destination] == "true"));
            bool showDelta = (fakedestination != "" && (Configuration.singleton()["catWatcherShowDelta", destination] == "true"));

            if (destination == ">TWITTER<")
            {
                shortenUrls = true;
                showDelta = true;
            }

            if (forceShowAll)
                showDelta = false;

            string message;

            
            if (items != null && items.Count > 0)
            {
                string listString = "";
                foreach (string item in items)
                {
                    if (!shortenUrls)
                    {
                        listString += "[[" + item + "]]";
                    }
                    else
                    {
                        try
                        {
                            Uri uri = new Uri(Configuration.singleton()["wikiUrl"] + item);
                            listString += IsGd.shorten(uri).ToString();
                        }
                        catch (UriFormatException ex)
                        {
                            listString += Configuration.singleton()["wikiUrl"] + item;
                            GlobalFunctions.errorLog(ex);
                        }
                    }

                    if (showWaitTime)
                    {
                        DAL.Select q = new DAL.Select("item_entrytime");
                        q.addWhere(new DAL.WhereConds("item_name", item));
                        q.addWhere(new DAL.WhereConds("item_keyword", keyword));
                        q.setFrom("categoryitems");

                        string insertDate = DAL.singleton().executeScalarSelect(q);
                        DateTime realInsertDate;
                        if (!DateTime.TryParse(insertDate, out realInsertDate))
                            realInsertDate = DateTime.Now;

                        TimeSpan ts = DateTime.Now - realInsertDate;

                        if (ts >= minimumWaitTime)
                        {
                            string[] messageparams = {
                                                         ts.Hours.ToString().PadLeft(2, '0'),
                                                         ts.Minutes.ToString().PadLeft(2, '0'),
                                                         ts.Seconds.ToString().PadLeft(2, '0'),
                                                         ts.TotalDays >= 1 ? ((int)Math.Floor(ts.TotalDays)) + "d " : ""
                                                     };
                            listString += new Message().get("catWatcherWaiting", messageparams);
                        }
                    }

                    // trailing space added as a hack because MediaWiki doesn't preserve the trailing space :(
                    listString += new Message().get("listSeparator") + " ";
                }
                listString = listString.TrimEnd(' ', ',');
                string pluralString = items.Count == 1 ? new Message().get(keyword + "Singular", "keywordSingularDefault") : new Message().get(keyword + "Plural", "keywordPluralDefault");
                string[] messageParams = {items.Count.ToString(), pluralString, listString};
                message = new Message().get(keyword + (showDelta ? "New" : "") + "HasItems",
                                                               messageParams);
            }
            else
            {
                string[] mp = {new Message().get(keyword + "Plural", "keywordPluralDefault")};
                message = new Message().get(keyword + "NoItems", mp);
            }
            return message;
        }

        private CategoryWatcher getWatcher(string keyword)
        {
            CategoryWatcher cw;
            bool success = this._watchers.TryGetValue(keyword, out cw);
            return success ? cw : null;
        }

        /// <summary>
        /// Gets the keywords.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, CategoryWatcher>.KeyCollection getKeywords()
        {
            return this._watchers.Keys;
        }

        private ArrayList removeBlacklistedItems(ArrayList pageList)
        {
            DAL.Select q = new DAL.Select("ip_title");
            q.setFrom("ignoredpages");
            ArrayList blacklist = DAL.singleton().executeSelect(q);

            foreach (object[] item in blacklist)
            {
                if (pageList.Contains(item[0]))
                {
                    pageList.Remove(item[0]);
                }
            }

            return pageList;
        }

        /// <summary>
        /// Sets the delay.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="newDelay">The new delay.</param>
        /// <returns></returns>
        public CommandResponseHandler setDelay(string keyword, int newDelay)
        {
            if (newDelay < 1)
            {
                string message = new Message().get("delayTooShort");
                return new CommandResponseHandler(message);
            }

            CategoryWatcher cw = getWatcher(keyword);
            if (cw != null)
            {
                Dictionary<string, string> vals = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "watcher_sleeptime",
                                                              newDelay.ToString( )
                                                              }
                                                      };
                DAL.singleton().update("watcher", vals, 0, new DAL.WhereConds("watcher_keyword", keyword));
                cw.sleepTime = newDelay;
                return new CommandResponseHandler(new Message().get("done"));
            }
            return new CommandResponseHandler();
        }

        /// <summary>
        /// Gets the delay.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns></returns>
        public int getDelay(string keyword)
        {
            CategoryWatcher cw = getWatcher(keyword);
            if (cw != null)
            {
                return cw.sleepTime;
            }
            return 0;
        }

        private static int getWatcherId(string keyword)
        {
            DAL.Select q = new DAL.Select("watcher_id");
            q.setFrom("watcher");
            q.addWhere(new DAL.WhereConds("watcher_keyword", keyword));
            string watcherIdString = DAL.singleton().executeScalarSelect(q);

            return int.Parse(watcherIdString);
        }


        /// <summary>
        /// Determines whether [is watcher in channel] [the specified channel].
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        /// 	<c>true</c> if [is watcher in channel] [the specified channel]; otherwise, <c>false</c>.
        /// </returns>
        public bool isWatcherInChannel(string channel, string keyword)
        {
            DAL.Select q = new DAL.Select("COUNT(*)");
            q.setFrom("channelwatchers");
            q.addWhere(new DAL.WhereConds("channel_name", channel));
            q.addWhere(new DAL.WhereConds("watcher_keyword", keyword));
            q.addJoin("channel", DAL.Select.JoinTypes.Inner,
                      new DAL.WhereConds(false, "cw_channel", "=", false, "channel_id"));
            q.addJoin("watcher", DAL.Select.JoinTypes.Inner,
                      new DAL.WhereConds(false, "cw_watcher", "=", false, "watcher_id"));

            string count = DAL.singleton().executeScalarSelect(q);
            if (count == "0")
                return false;
            return true;
        }
    }
}