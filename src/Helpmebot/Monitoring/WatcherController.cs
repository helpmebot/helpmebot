// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WatcherController.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Controls instances of CategoryWatchers for the bot
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Monitoring
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;

    using Microsoft.Practices.ServiceLocation;

    using MySql.Data.MySqlClient;

    /// <summary>
    ///   Controls instances of CategoryWatchers for the bot
    /// </summary>
    internal class WatcherController
    {
        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        /// The watchers.
        /// </summary>
        private readonly Dictionary<string, CategoryWatcher> watchers;

        /// <summary>
        /// Initialises a new instance of the <see cref="WatcherController"/> class.
        /// </summary>
        protected WatcherController()
        {
            this.watchers = new Dictionary<string, CategoryWatcher>();

            DAL.Select q = new DAL.Select("watcher_category", "watcher_keyword", "watcher_sleeptime");
            q.addOrder(new DAL.Select.Order("watcher_priority", true));
            q.setFrom("watcher");
            q.addLimit(100, 0);
            ArrayList watchersInDb = DAL.singleton().executeSelect(q);
            foreach (object[] item in watchersInDb)
            {
                this.watchers.Add(
                    (string)item[1],
                    new CategoryWatcher((string)item[0], (string)item[1], int.Parse(((uint)item[2]).ToString(CultureInfo.InvariantCulture))));
            }

            foreach (KeyValuePair<string, CategoryWatcher> item in this.watchers)
            {
                item.Value.CategoryHasItemsEvent += CategoryHasItemsEvent;
            }
        }

        /// <summary>
        /// The instance.
        /// </summary>
        /// <returns>
        /// The <see cref="WatcherController"/>.
        /// </returns>
        public static WatcherController Instance()
        {
            return instance ?? (instance = new WatcherController());
        }

        /// <summary>
        /// The _instance.
        /// </summary>
        private static WatcherController instance;

        /// <summary>
        /// Determines whether the specified word is a valid keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        ///     <c>true</c> if the specified word is a valid keyword; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidKeyword(string keyword)
        {
            return this.watchers.ContainsKey(keyword);
        }

        /// <summary>
        /// Adds the watcher to channel.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>The bool</returns>
        public bool AddWatcherToChannel(string keyword, string channel)
        {
            string channelId = LegacyConfig.singleton().getChannelId(channel);
            int watcherId = GetWatcherId(keyword);

            DAL.Select q = new DAL.Select("COUNT(*)");
            q.setFrom("channelwatchers");
            q.addWhere(new DAL.WhereConds("cw_channel", channelId));
            q.addWhere(new DAL.WhereConds("cw_watcher", watcherId));
            string count = DAL.singleton().executeScalarSelect(q);

            if (count == "0")
            {
                DAL.singleton().insert("channelwatchers", channelId, watcherId.ToString(CultureInfo.InvariantCulture));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the watcher from channel.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="channel">The channel.</param>
        public void RemoveWatcherFromChannel(string keyword, string channel)
        {
            string channelId = LegacyConfig.singleton().getChannelId(channel);
            int watcherId = GetWatcherId(keyword);

            DAL.singleton()
                .delete(
                    "channelwatchers",
                    0,
                    new DAL.WhereConds("cw_channel", channelId),
                    new DAL.WhereConds("cw_watcher", watcherId));
        }

        /// <summary>
        /// Forces the update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>the compile</returns>
        public string ForceUpdate(string key, string destination)
        {
            CategoryWatcher cw;
            if (this.watchers.TryGetValue(key, out cw))
            {
                List<string> items = cw.DoCategoryCheck().ToList();
                UpdateDatabaseTable(items, key);
                return CompileMessage(items, key, destination, true);
            }

            return null;
        }

        /// <summary>
        /// The category has items event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void CategoryHasItemsEvent(object sender, CategoryHasItemsEventArgs e)
        {
            List<string> items = e.Items.ToList();

            IEnumerable<string> newItems = UpdateDatabaseTable(items, e.Keyword);

            DAL.Select q = new DAL.Select("channel_name");
            q.addJoin(
                "channelwatchers",
                DAL.Select.JoinTypes.Inner,
                new DAL.WhereConds(false, "watcher_id", "=", false, "cw_watcher"));
            q.addJoin(
                "channel",
                DAL.Select.JoinTypes.Inner,
                new DAL.WhereConds(false, "channel_id", "=", false, "cw_channel"));
            q.setFrom("watcher");
            q.addWhere(new DAL.WhereConds("watcher_keyword", e.Keyword));
            q.addLimit(10, 0);

            ArrayList channels = DAL.singleton().executeSelect(q);
            foreach (object[] item in channels)
            {
                string channel = (string)item[0];

                string message = CompileMessage(items, e.Keyword, channel, false);
                if (LegacyConfig.singleton()["silence", channel] == "false")
                {
                    Helpmebot6.irc.IrcPrivmsg(channel, message);
                }
            }
        }

        /// <summary>
        /// The update database table.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private static IEnumerable<string> UpdateDatabaseTable(IEnumerable<string> items, string keyword)
        {
            List<string> newItems = new List<string>();
            foreach (string item in items)
            {
                DAL.Select q = new DAL.Select("COUNT(*)");
                q.setFrom("categoryitems");
                q.addWhere(new DAL.WhereConds("item_name", item));
                q.addWhere(new DAL.WhereConds("item_keyword", keyword));

                string databaseResult;
                try
                {
                    databaseResult = DAL.singleton().executeScalarSelect(q);
                }
                catch (MySqlException ex)
                {
                    ServiceLocator.Current.GetInstance<ILogger>().Error(ex.Message, ex);
                    databaseResult = "0";
                }

                if (databaseResult == "0")
                {
                    DAL.singleton().insert("categoryitems", string.Empty, item, string.Empty, keyword, "1");
                    newItems.Add(item);
                }
                else
                {
                    Dictionary<string, string> v = new Dictionary<string, string> { { "item_updateflag", "1" } };
                    DAL.singleton()
                        .update(
                            "categoryitems",
                            v,
                            1,
                            new DAL.WhereConds("item_name", item),
                            new DAL.WhereConds("item_keyword", keyword));
                }
            }

            DAL.singleton()
                .delete(
                    "categoryitems",
                    0,
                    new DAL.WhereConds("item_updateflag", 0),
                    new DAL.WhereConds("item_keyword", keyword));
            Dictionary<string, string> val = new Dictionary<string, string> { { "item_updateflag", "0" } };
            DAL.singleton().update("categoryitems", val, 0);

            return newItems;
        }

        /// <summary>
        /// The compile message.
        /// </summary>
        /// <param name="itemsEnumerable">
        /// The items enumerable.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="forceShowAll">
        /// The force show all.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string CompileMessage(IEnumerable<string> itemsEnumerable, string keyword, string destination, bool forceShowAll)
        {
            //// keywordHasItems: 0: count, 1: plural word(s), 2: items in category
            //// keywordNoItems: 0: plural word(s)
            //// keywordPlural
            //// keywordSingular

            List<string> items = itemsEnumerable.ToList();

            string fakedestination = destination;

            bool showWaitTime = fakedestination != string.Empty && (LegacyConfig.singleton()["showWaitTime", destination] == "true");

            TimeSpan minimumWaitTime;
            if (!TimeSpan.TryParse(LegacyConfig.singleton()["minimumWaitTime", destination], out minimumWaitTime))
            {
                minimumWaitTime = new TimeSpan(0);
            }

            bool shortenUrls = fakedestination != string.Empty && (LegacyConfig.singleton()["useShortUrlsInsteadOfWikilinks", destination] == "true");
            bool showDelta = fakedestination != string.Empty && (LegacyConfig.singleton()["catWatcherShowDelta", destination] == "true");

            if (forceShowAll)
            {
                showDelta = false;
            }

            string message;
            
            if (items.Any())
            {
                string listString = string.Empty;
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
                            Uri uri = new Uri(LegacyConfig.singleton()["wikiUrl"] + item);
                            listString += IsGd.shorten(uri).ToString();
                        }
                        catch (UriFormatException ex)
                        {
                            listString += LegacyConfig.singleton()["wikiUrl"] + item;
                            ServiceLocator.Current.GetInstance<ILogger>().Error(ex.Message, ex);
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
                        {
                            realInsertDate = DateTime.Now;
                        }

                        TimeSpan ts = DateTime.Now - realInsertDate;

                        if (ts >= minimumWaitTime)
                        {
                            string[] messageparams =
                                {
                                    ts.Hours.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                                    ts.Minutes.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                                    ts.Seconds.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                                    ts.TotalDays >= 1
                                        ? ((int)Math.Floor(ts.TotalDays)) + "d "
                                        : string.Empty
                                };
                            listString += new Message().GetMessage("catWatcherWaiting", messageparams);
                        }
                    }

                    // trailing space added as a hack because MediaWiki doesn't preserve the trailing space :(
                    listString += new Message().GetMessage("listSeparator") + " ";
                }

                listString = listString.TrimEnd(' ', ',');
                string pluralString = items.Count() == 1 ? new Message().GetMessage(keyword + "Singular", "keywordSingularDefault") : new Message().GetMessage(keyword + "Plural", "keywordPluralDefault");
                string[] messageParams =
                    {
                        items.Count().ToString(CultureInfo.InvariantCulture), pluralString,
                        listString
                    };
                message = new Message().GetMessage(keyword + (showDelta ? "New" : string.Empty) + "HasItems", messageParams);
            }
            else
            {
                string[] mp = { new Message().GetMessage(keyword + "Plural", "keywordPluralDefault") };
                message = new Message().GetMessage(keyword + "NoItems", mp);
            }

            return message;
        }

        /// <summary>
        /// The get watcher.
        /// </summary>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <returns>
        /// The <see cref="CategoryWatcher"/>.
        /// </returns>
        private CategoryWatcher GetWatcher(string keyword)
        {
            CategoryWatcher cw;
            bool success = this.watchers.TryGetValue(keyword, out cw);
            return success ? cw : null;
        }

        /// <summary>
        /// Gets the keywords.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, CategoryWatcher>.KeyCollection getKeywords()
        {
            return this.watchers.Keys;
        }

        /// <summary>
        /// Sets the delay.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="newDelay">The new delay.</param>
        /// <returns></returns>
        public CommandResponseHandler SetDelay(string keyword, int newDelay)
        {
            if (newDelay < 1)
            {
                string message = new Message().GetMessage("delayTooShort");
                return new CommandResponseHandler(message);
            }

            CategoryWatcher cw = this.GetWatcher(keyword);
            if (cw != null)
            {
                Dictionary<string, string> vals = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "watcher_sleeptime",
                                                              newDelay.ToString(
                                                                  CultureInfo.InvariantCulture)
                                                          }
                                                      };
                DAL.singleton().update("watcher", vals, 0, new DAL.WhereConds("watcher_keyword", keyword));
                cw.SleepTime = newDelay;
                return new CommandResponseHandler(new Message().GetMessage("done"));
            }

            return new CommandResponseHandler();
        }

        /// <summary>
        /// Gets the delay.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns></returns>
        public int GetDelay(string keyword)
        {
            CategoryWatcher cw = this.GetWatcher(keyword);
            if (cw != null)
            {
                return cw.SleepTime;
            }

            return 0;
        }

        /// <summary>
        /// The get watcher id.
        /// </summary>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private static int GetWatcherId(string keyword)
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
        ///     <c>true</c> if [is watcher in channel] [the specified channel]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWatcherInChannel(string channel, string keyword)
        {
            DAL.Select q = new DAL.Select("COUNT(*)");
            q.setFrom("channelwatchers");
            q.addWhere(new DAL.WhereConds("channel_name", channel));
            q.addWhere(new DAL.WhereConds("watcher_keyword", keyword));
            q.addJoin(
                "channel",
                DAL.Select.JoinTypes.Inner,
                new DAL.WhereConds(false, "cw_channel", "=", false, "channel_id"));
            q.addJoin(
                "watcher",
                DAL.Select.JoinTypes.Inner,
                new DAL.WhereConds(false, "cw_watcher", "=", false, "watcher_id"));

            string count = DAL.singleton().executeScalarSelect(q);
            if (count == "0")
            {
                return false;
            }

            return true;
        }
    }
}