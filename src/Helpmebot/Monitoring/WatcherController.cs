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
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using MySql.Data.MySqlClient;

    /// <summary>
    ///   Controls instances of CategoryWatchers for the bot
    /// </summary>
    internal class WatcherController
    {
        /// <summary>
        /// The _instance.
        /// </summary>
        private static WatcherController instance;

        /// <summary>
        /// The watchers.
        /// </summary>
        private readonly Dictionary<string, CategoryWatcher> watchers;

        /// <summary>
        /// The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// The url shortening service.
        /// </summary>
        private readonly IUrlShorteningService urlShorteningService;

        /// <summary>
        /// Initialises a new instance of the <see cref="WatcherController"/> class.
        /// </summary>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        /// <param name="urlShorteningService">
        /// The url Shortening Service.
        /// </param>
        protected WatcherController(IMessageService messageService, IUrlShorteningService urlShorteningService)
        {
            this.messageService = messageService;
            this.urlShorteningService = urlShorteningService;
            this.watchers = new Dictionary<string, CategoryWatcher>();

            var q = new LegacyDatabase.Select("watcher_category", "watcher_keyword", "watcher_sleeptime");
            q.AddOrder(new LegacyDatabase.Select.Order("watcher_priority", true));
            q.SetFrom("watcher");
            q.AddLimit(100, 0);
            ArrayList watchersInDb = LegacyDatabase.Singleton().ExecuteSelect(q);
            foreach (object[] item in watchersInDb)
            {
                this.watchers.Add(
                    (string)item[1],
                    new CategoryWatcher((string)item[0], (string)item[1], int.Parse(((uint)item[2]).ToString(CultureInfo.InvariantCulture))));
            }

            foreach (KeyValuePair<string, CategoryWatcher> item in this.watchers)
            {
                item.Value.CategoryHasItemsEvent += this.CategoryHasItemsEvent;
            }
        }

        #region Public methods

        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        /// The instance.
        /// </summary>
        /// <returns>
        /// The <see cref="WatcherController"/>.
        /// </returns>
        public static WatcherController Instance()
        {
            if (instance == null)
            {
                // FIXME: ServiceLocator usages
                var ms = ServiceLocator.Current.GetInstance<IMessageService>();
                var ss = ServiceLocator.Current.GetInstance<IUrlShorteningService>();

                instance = new WatcherController(ms, ss);
            }

            return instance;
        }

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
            string channelId = LegacyConfig.Singleton().GetChannelId(channel);
            int watcherId = GetWatcherId(keyword);

            var q = new LegacyDatabase.Select("COUNT(*)");
            q.SetFrom("channelwatchers");
            q.AddWhere(new LegacyDatabase.WhereConds("cw_channel", channelId));
            q.AddWhere(new LegacyDatabase.WhereConds("cw_watcher", watcherId));
            string count = LegacyDatabase.Singleton().ExecuteScalarSelect(q);

            if (count == "0")
            {
                LegacyDatabase.Singleton().Insert("channelwatchers", channelId, watcherId.ToString(CultureInfo.InvariantCulture));
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
            string channelId = LegacyConfig.Singleton().GetChannelId(channel);
            int watcherId = GetWatcherId(keyword);

            var deleteCommand =
                new MySqlCommand("DELETE FROM channelwatchers WHERE cw_channel = @channel AND cw_watcher = @watcher;");
            deleteCommand.Parameters.AddWithValue("@channel", channelId);
            deleteCommand.Parameters.AddWithValue("@watcher", watcherId);
            LegacyDatabase.Singleton().ExecuteCommand(deleteCommand);
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
                return this.CompileMessage(items, key, destination, true);
            }

            return null;
        }

        /// <summary>
        /// Gets the keywords.
        /// </summary>
        /// <returns>
        /// The list of keywords
        /// </returns>
        public Dictionary<string, CategoryWatcher>.KeyCollection GetKeywords()
        {
            return this.watchers.Keys;
        }

        /// <summary>
        /// Sets the delay.
        /// </summary>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <param name="newDelay">
        /// The new delay.
        /// </param>
        /// <param name="messageContext">
        /// The message Context.
        /// </param>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>
        /// </returns>
        public CommandResponseHandler SetDelay(string keyword, int newDelay, object messageContext)
        {
            if (newDelay < 1)
            {
                string message = this.messageService.RetrieveMessage("delayTooShort", messageContext, null);
                return new CommandResponseHandler(message);
            }

            CategoryWatcher cw = this.GetWatcher(keyword);
            if (cw != null)
            {
                var vals = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "watcher_sleeptime",
                                                              newDelay.ToString(
                                                                  CultureInfo.InvariantCulture)
                                                          }
                                                      };
                LegacyDatabase.Singleton().Update("watcher", vals, 0, new LegacyDatabase.WhereConds("watcher_keyword", keyword));
                cw.SleepTime = newDelay;
                return new CommandResponseHandler(this.messageService.RetrieveMessage(Messages.Done, messageContext, null));
            }

            return new CommandResponseHandler();
        }

        /// <summary>
        /// The get delay.
        /// </summary>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
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
        /// Determines whether [is watcher in channel] [the specified channel].
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        ///     <c>true</c> if [is watcher in channel] [the specified channel]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWatcherInChannel(string channel, string keyword)
        {
            var q = new LegacyDatabase.Select("COUNT(*)");
            q.SetFrom("channelwatchers");
            q.AddWhere(new LegacyDatabase.WhereConds("channel_name", channel));
            q.AddWhere(new LegacyDatabase.WhereConds("watcher_keyword", keyword));
            q.AddJoin(
                "channel",
                LegacyDatabase.Select.JoinTypes.Inner,
                new LegacyDatabase.WhereConds(false, "cw_channel", "=", false, "channel_id"));
            q.AddJoin(
                "watcher",
                LegacyDatabase.Select.JoinTypes.Inner,
                new LegacyDatabase.WhereConds(false, "cw_watcher", "=", false, "watcher_id"));

            var count = LegacyDatabase.Singleton().ExecuteScalarSelect(q);
            return count != "0";
        }

        #endregion

        /// <summary>
        /// The update database table.
        /// </summary>
        /// <param name="items">
        ///     The items.
        /// </param>
        /// <param name="keyword">
        ///     The keyword.
        /// </param>
        private static void UpdateDatabaseTable(IEnumerable<string> items, string keyword)
        {
            var newItems = new List<string>();
            foreach (var item in items)
            {
                var q = new LegacyDatabase.Select("COUNT(*)");
                q.SetFrom("categoryitems");
                q.AddWhere(new LegacyDatabase.WhereConds("item_name", item));
                q.AddWhere(new LegacyDatabase.WhereConds("item_keyword", keyword));

                string databaseResult;
                try
                {
                    databaseResult = LegacyDatabase.Singleton().ExecuteScalarSelect(q);
                }
                catch (MySqlException ex)
                {
                    ServiceLocator.Current.GetInstance<ILogger>().Error(ex.Message, ex);
                    databaseResult = "0";
                }

                if (databaseResult == "0")
                {
                    LegacyDatabase.Singleton().Insert("categoryitems", string.Empty, item, string.Empty, keyword, "1");
                    newItems.Add(item);
                }
                else
                {
                    var v = new Dictionary<string, string> { { "item_updateflag", "1" } };
                    LegacyDatabase.Singleton()
                        .Update(
                            "categoryitems",
                            v,
                            1,
                            new LegacyDatabase.WhereConds("item_name", item),
                            new LegacyDatabase.WhereConds("item_keyword", keyword));
                }
            }

            var deleteCommand =
                new MySqlCommand("DELETE FROM categoryitems WHERE item_updateflag = 0 AND item_keyword = @keyword;");
            deleteCommand.Parameters.AddWithValue("@update", 0);
            deleteCommand.Parameters.AddWithValue("@keyword", keyword);
            LegacyDatabase.Singleton().ExecuteCommand(deleteCommand);

            var val = new Dictionary<string, string> { { "item_updateflag", "0" } };
            LegacyDatabase.Singleton().Update("categoryitems", val, 0);
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
            var q = new LegacyDatabase.Select("watcher_id");
            q.SetFrom("watcher");
            q.AddWhere(new LegacyDatabase.WhereConds("watcher_keyword", keyword));
            string watcherIdString = LegacyDatabase.Singleton().ExecuteScalarSelect(q);

            return int.Parse(watcherIdString);
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
        private void CategoryHasItemsEvent(object sender, CategoryHasItemsEventArgs e)
        {
            List<string> items = e.Items.ToList();

            UpdateDatabaseTable(items, e.Keyword);

            var q = new LegacyDatabase.Select("channel_name");
            q.AddJoin(
                "channelwatchers",
                LegacyDatabase.Select.JoinTypes.Inner,
                new LegacyDatabase.WhereConds(false, "watcher_id", "=", false, "cw_watcher"));
            q.AddJoin(
                "channel",
                LegacyDatabase.Select.JoinTypes.Inner,
                new LegacyDatabase.WhereConds(false, "channel_id", "=", false, "cw_channel"));
            q.SetFrom("watcher");
            q.AddWhere(new LegacyDatabase.WhereConds("watcher_keyword", e.Keyword));
            q.AddLimit(10, 0);

            ArrayList channels = LegacyDatabase.Singleton().ExecuteSelect(q);
            foreach (object[] item in channels)
            {
                var channel = (string)item[0];

                string message = this.CompileMessage(items, e.Keyword, channel, false);
                if (LegacyConfig.Singleton()["silence", channel] == "false")
                {
                    Helpmebot6.irc.IrcPrivmsg(channel, message);
                }
            }
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
        private string CompileMessage(IEnumerable<string> itemsEnumerable, string keyword, string destination, bool forceShowAll)
        {
            //// keywordHasItems: 0: count, 1: plural word(s), 2: items in category
            //// keywordNoItems: 0: plural word(s)
            //// keywordPlural
            //// keywordSingular

            List<string> items = itemsEnumerable.ToList();

            string fakedestination = destination;

            bool showWaitTime = fakedestination != string.Empty && (LegacyConfig.Singleton()["showWaitTime", destination] == "true");

            TimeSpan minimumWaitTime;
            if (!TimeSpan.TryParse(LegacyConfig.Singleton()["minimumWaitTime", destination], out minimumWaitTime))
            {
                minimumWaitTime = new TimeSpan(0);
            }

            bool shortenUrls = fakedestination != string.Empty && (LegacyConfig.Singleton()["useShortUrlsInsteadOfWikilinks", destination] == "true");
            bool showDelta = fakedestination != string.Empty && (LegacyConfig.Singleton()["catWatcherShowDelta", destination] == "true");

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
                    // Display [[]]'ied name of the page which requests help
                    listString += "[[" + item + "]] ";
                    
                    // Display an http URL to the page, if desired
                    if (shortenUrls)
                    {
                        var uriString = LegacyConfig.Singleton()["wikiUrl"] + item;
                        listString += this.urlShorteningService.Shorten(uriString);
                    }

                    if (showWaitTime)
                    {
                        var q = new LegacyDatabase.Select("item_entrytime");
                        q.AddWhere(new LegacyDatabase.WhereConds("item_name", item));
                        q.AddWhere(new LegacyDatabase.WhereConds("item_keyword", keyword));
                        q.SetFrom("categoryitems");

                        string insertDate = LegacyDatabase.Singleton().ExecuteScalarSelect(q);
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
                            listString += this.messageService.RetrieveMessage("catWatcherWaiting", destination, messageparams);
                        }
                    }

                    // trailing space added as a hack because MediaWiki doesn't preserve the trailing space :(
                    listString += this.messageService.RetrieveMessage("listSeparator", destination, null) + " ";
                }

                listString = listString.TrimEnd(' ', ',');
                string pluralString = items.Count() == 1
                                          ? this.messageService.RetrieveMessage(
                                              keyword + "Singular",
                                              destination,
                                              new[] { "keywordSingularDefault" })
                                          : this.messageService.RetrieveMessage(
                                              keyword + "Plural",
                                              destination,
                                              new[] { "keywordPluralDefault" });
                string[] messageParams =
                    {
                        items.Count().ToString(CultureInfo.InvariantCulture), pluralString,
                        listString
                    };
                message = this.messageService.RetrieveMessage(keyword + (showDelta ? "New" : string.Empty) + "HasItems", destination, messageParams);
            }
            else
            {
                string[] mp = { this.messageService.RetrieveMessage(keyword + "Plural", destination, new[] { "keywordPluralDefault" }) };
                message = this.messageService.RetrieveMessage(keyword + "NoItems", destination, mp);
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
    }
}
