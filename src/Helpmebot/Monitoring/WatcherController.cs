﻿// --------------------------------------------------------------------------------------------------------------------
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
// --------------------------------------------------------------------------------------------------------------------

using System.Net;

namespace Helpmebot.Monitoring
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using Castle.Core.Logging;

    using Stwalkerster.IrcClient.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using MySql.Data.MySqlClient;

    /// <summary>
    ///     Controls instances of CategoryWatchers for the bot
    /// </summary>
    internal class WatcherController
    {
        #region Static Fields

        /// <summary>
        ///     The _instance.
        /// </summary>
        private static WatcherController instance;

        #endregion

        #region Fields

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The IRC client.
        /// </summary>
        private readonly IIrcClient ircClient;

        /// <summary>
        ///     The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        ///     The url shortening service.
        /// </summary>
        private readonly IUrlShorteningService urlShorteningService;

        /// <summary>
        ///     The watchers.
        /// </summary>
        private readonly Dictionary<string, CategoryWatcher> watchers;

        /// <summary>
        /// The legacy database.
        /// </summary>
        private readonly ILegacyDatabase legacyDatabase;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="WatcherController"/> class.
        /// </summary>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        /// <param name="urlShorteningService">
        /// The url Shortening Service.
        /// </param>
        /// <param name="watchedCategoryRepository">
        /// The watched Category Repository.
        /// </param>
        /// <param name="mediaWikiSiteRepository">
        /// The media Wiki Site Repository.
        /// </param>
        /// <param name="ignoredPagesRepository">
        /// The ignored Pages Repository.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="ircClient">
        /// The IRC Client.
        /// </param>
        /// <param name="legacyDatabase">
        /// The legacy Database.
        /// </param>
        protected WatcherController(
            IMessageService messageService, 
            IUrlShorteningService urlShorteningService, 
            IWatchedCategoryRepository watchedCategoryRepository, 
            IMediaWikiSiteRepository mediaWikiSiteRepository,
            IIgnoredPagesRepository ignoredPagesRepository,
            ILogger logger,
            IIrcClient ircClient,
            ILegacyDatabase legacyDatabase)
        {
            this.messageService = messageService;
            this.urlShorteningService = urlShorteningService;
            this.watchers = new Dictionary<string, CategoryWatcher>();
            this.logger = logger;
            this.ircClient = ircClient;

            foreach (WatchedCategory item in watchedCategoryRepository.Get())
            {
                var categoryWatcher = new CategoryWatcher(
                    item,
                    mediaWikiSiteRepository,
                    ignoredPagesRepository,
                    logger.CreateChildLogger("CategoryWatcher[" + item.Keyword + "]"));
                this.watchers.Add(item.Keyword, categoryWatcher);
                categoryWatcher.CategoryHasItemsEvent += this.CategoryHasItemsEvent;
            }

            this.legacyDatabase = legacyDatabase;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The instance.
        /// </summary>
        /// <returns>
        ///     The <see cref="WatcherController" />.
        /// </returns>
        public static WatcherController Instance()
        {
            if (instance == null)
            {
                // FIXME: ServiceLocator - ALL THE THINGS!
                var ms = ServiceLocator.Current.GetInstance<IMessageService>();
                var ss = ServiceLocator.Current.GetInstance<IUrlShorteningService>();
                var wcrepo = ServiceLocator.Current.GetInstance<IWatchedCategoryRepository>();
                var mwrepo = ServiceLocator.Current.GetInstance<IMediaWikiSiteRepository>();
                var iprepo = ServiceLocator.Current.GetInstance<IIgnoredPagesRepository>();
                var logger = ServiceLocator.Current.GetInstance<ILogger>();
                var irc = ServiceLocator.Current.GetInstance<IIrcClient>();
                var legacyDb = ServiceLocator.Current.GetInstance<ILegacyDatabase>();

                instance = new WatcherController(ms, ss, wcrepo, mwrepo, iprepo, logger, irc, legacyDb);
            }

            return instance;
        }

        /// <summary>
        /// Adds the watcher to channel.
        /// </summary>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The bool
        /// </returns>
        public bool AddWatcherToChannel(string keyword, string channel)
        {
            string channelId = LegacyConfig.Singleton().GetChannelId(channel);
            int watcherId = this.GetWatcherId(keyword);

            var countCommand =
                new MySqlCommand(
                    "SELECT COUNT(*) FROM channelwatchers WHERE cw_channel = @channel AND cw_watcher = @watcher;");
            countCommand.Parameters.AddWithValue("@channel", channelId);
            countCommand.Parameters.AddWithValue("@watcher", watcherId);
            string count = this.legacyDatabase.ExecuteScalarSelect(countCommand);

            if (count == "0")
            {
                var command = new MySqlCommand("INSERT INTO channelwatchers VALUES ( @channelid, @watcherid );");
                command.Parameters.AddWithValue("@channelid", channelId);
                command.Parameters.AddWithValue("@watcherid", watcherId.ToString(CultureInfo.InvariantCulture));

                this.legacyDatabase.ExecuteCommand(command);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Forces the update.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <returns>
        /// the compile
        /// </returns>
        public string ForceUpdate(string key, string destination)
        {
            this.logger.InfoFormat("Forcing update for {0} at {1}.", key, destination);

            CategoryWatcher cw;
            try
            {
                if (this.watchers.TryGetValue(key, out cw))
                {
                    List<string> items = cw.DoCategoryCheck().ToList();
                    this.UpdateDatabaseTable(items, key);
                    return this.CompileMessage(items, key, destination, true);
                }
            }
            catch (WebException ex)
            {
                return "Unable to contact Wikipedia API: " + ex.Message;
            }

            return null;
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
        ///     Gets the keywords.
        /// </summary>
        /// <returns>
        ///     The list of keywords
        /// </returns>
        public Dictionary<string, CategoryWatcher>.KeyCollection GetKeywords()
        {
            return this.watchers.Keys;
        }

        /// <summary>
        /// Determines whether the specified word is a valid keyword.
        /// </summary>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified word is a valid keyword; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidKeyword(string keyword)
        {
            return this.watchers.ContainsKey(keyword);
        }

        /// <summary>
        /// Determines whether [is watcher in channel] [the specified channel].
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <returns>
        /// <c>true</c> if [is watcher in channel] [the specified channel]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWatcherInChannel(string channel, string keyword)
        {
            var command = new MySqlCommand("SELECT COUNT(*) FROM channelwatchers INNER JOIN channel ON cw_channel = channel_id INNER JOIN watcher ON cw_watcher = watcher_id WHERE channel_name = @channel AND watcher_keyword = @keyword;");
            command.Parameters.AddWithValue("@channel", channel);
            command.Parameters.AddWithValue("@keyword", keyword);
            string count = this.legacyDatabase.ExecuteScalarSelect(command);
            return count != "0";
        }

        /// <summary>
        /// Removes the watcher from channel.
        /// </summary>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void RemoveWatcherFromChannel(string keyword, string channel)
        {
            string channelId = LegacyConfig.Singleton().GetChannelId(channel);
            int watcherId = this.GetWatcherId(keyword);

            var deleteCommand =
                new MySqlCommand("DELETE FROM channelwatchers WHERE cw_channel = @channel AND cw_watcher = @watcher;");
            deleteCommand.Parameters.AddWithValue("@channel", channelId);
            deleteCommand.Parameters.AddWithValue("@watcher", watcherId);
            this.legacyDatabase.ExecuteCommand(deleteCommand);
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
                var command = new MySqlCommand("UPDATE watcher SET watcher_sleeptime = @value WHERE watcher_keyword = @name LIMIT 1;");

                command.Parameters.AddWithValue("@value", newDelay.ToString(CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@name", keyword);

                this.legacyDatabase.ExecuteCommand(command);

                cw.SleepTime = newDelay;
                return
                    new CommandResponseHandler(this.messageService.RetrieveMessage(Messages.Done, messageContext, null));
            }

            return new CommandResponseHandler();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The update database table.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        private void UpdateDatabaseTable(IEnumerable<string> items, string keyword)
        {
            var newItems = new List<string>();
            foreach (string item in items)
            {
                var countCommand =
                    new MySqlCommand(
                        "SELECT COUNT(*) FROM categoryitems WHERE item_name = @name AND item_keyword = @keyword;");
                countCommand.Parameters.AddWithValue("@name", item);
                countCommand.Parameters.AddWithValue("@keyword", keyword);

                string databaseResult;
                try
                {
                    databaseResult = this.legacyDatabase.ExecuteScalarSelect(countCommand);
                }
                catch (MySqlException ex)
                {
                    ServiceLocator.Current.GetInstance<ILogger>().Error(ex.Message, ex);
                    databaseResult = "0";
                }

                if (databaseResult == "0")
                {
                    var command = new MySqlCommand("INSERT INTO categoryitems VALUES (null, @item, null, @keyword, 1);");
                    command.Parameters.AddWithValue("@item", item);
                    command.Parameters.AddWithValue("@keyword", keyword);

                    this.legacyDatabase.ExecuteCommand(command);
                    newItems.Add(item);
                }
                else
                {
                    var command = new MySqlCommand("UPDATE categoryitems SET item_updateflag = 1 WHERE item_keyword = @keyword AND item_name = @name LIMIT 1;");

                    command.Parameters.AddWithValue("@name", item);
                    command.Parameters.AddWithValue("@keyword", keyword);

                    this.legacyDatabase.ExecuteCommand(command);
                }
            }

            var deleteCommand =
                new MySqlCommand("DELETE FROM categoryitems WHERE item_updateflag = 0 AND item_keyword = @keyword;");
            deleteCommand.Parameters.AddWithValue("@update", 0);
            deleteCommand.Parameters.AddWithValue("@keyword", keyword);
            this.legacyDatabase.ExecuteCommand(deleteCommand);

            var updateCommand = new MySqlCommand("UPDATE categoryitems SET item_updateflag = 0;");
            this.legacyDatabase.ExecuteCommand(updateCommand);
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

            this.UpdateDatabaseTable(items, e.Keyword);

            var query =
                new MySqlCommand(
                    "SELECT channel_name FROM `watcher` INNER JOIN `channelwatchers` ON watcher_id = cw_watcher INNER JOIN `channel` ON channel_id = cw_channel WHERE watcher_keyword = @keyword;");

            query.Parameters.AddWithValue("@keyword", e.Keyword);
            
            ArrayList channels = this.legacyDatabase.ExecuteSelect(query);
            foreach (object[] item in channels)
            {
                var channel = (string)item[0];

                string message = this.CompileMessage(items, e.Keyword, channel, false);
                if (LegacyConfig.Singleton()["silence", channel] == "false")
                {
                    this.ircClient.SendMessage(channel, message);
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
        private string CompileMessage(
            IEnumerable<string> itemsEnumerable, 
            string keyword, 
            string destination, 
            bool forceShowAll)
        {
            //// keywordHasItems: 0: count, 1: plural word(s), 2: items in category
            //// keywordNoItems: 0: plural word(s)
            //// keywordPlural
            //// keywordSingular
            List<string> items = itemsEnumerable.ToList();

            string fakedestination = destination;

            bool showWaitTime = fakedestination != string.Empty
                                && (LegacyConfig.Singleton()["showWaitTime", destination] == "true");

            TimeSpan minimumWaitTime;
            if (!TimeSpan.TryParse(LegacyConfig.Singleton()["minimumWaitTime", destination], out minimumWaitTime))
            {
                minimumWaitTime = new TimeSpan(0);
            }

            bool shortenUrls = fakedestination != string.Empty
                               && (LegacyConfig.Singleton()["useShortUrlsInsteadOfWikilinks", destination] == "true");
            bool showDelta = fakedestination != string.Empty
                             && (LegacyConfig.Singleton()["catWatcherShowDelta", destination] == "true");

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
                        string urlName = item.Replace(' ', '_');

                        string uriString = LegacyConfig.Singleton()["wikiUrl"] + HttpUtility.UrlEncode(urlName);
                        listString += this.urlShorteningService.Shorten(uriString);
                    }

                    if (showWaitTime)
                    {
                        var command =
                            new MySqlCommand(
                                "SELECT item_entrytime FROM categoryitems WHERE item_name = @name and item_keyword = @keyword;");

                        command.Parameters.AddWithValue("@name", item);
                        command.Parameters.AddWithValue("@keyword", keyword);

                        string insertDate = this.legacyDatabase.ExecuteScalarSelect(command);
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
                            listString += this.messageService.RetrieveMessage(
                                "catWatcherWaiting", 
                                destination, 
                                messageparams);
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
                message = this.messageService.RetrieveMessage(
                    keyword + (showDelta ? "New" : string.Empty) + "HasItems", 
                    destination, 
                    messageParams);
            }
            else
            {
                string[] mp =
                    {
                        this.messageService.RetrieveMessage(
                            keyword + "Plural", 
                            destination, 
                            new[] { "keywordPluralDefault" })
                    };
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

        /// <summary>
        /// The get watcher id.
        /// </summary>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int GetWatcherId(string keyword)
        {
            if (this.watchers.ContainsKey(keyword))
            {
                return this.watchers[keyword].WatchedCategory.Id;
            }

            return 0;
        }

        #endregion
    }
}