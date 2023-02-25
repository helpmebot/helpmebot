namespace Helpmebot.CategoryWatcher.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using CoreServices.Services.Messages;
    using Helpmebot.CategoryWatcher.Commands;
    using Helpmebot.CategoryWatcher.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using Prometheus;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;

    public class CategoryWatcherHelperService : ICategoryWatcherHelperService
    {
        private static readonly Gauge CategoryWatcherCount = Metrics.CreateGauge(
            "helpmebot_catwatcher_pages",
            "The number of pages in the catwatcher category",
            new GaugeConfiguration
            {
                LabelNames = new[] {"flag"}
            });
        
        private readonly ILinkerService linkerService;
        private readonly IUrlShorteningService urlShorteningService;
        private readonly ISession session;
        private readonly ILogger logger;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IWatcherConfigurationService watcherConfig;
        private readonly IItemPersistenceService watcherItemPersistence;

        public CategoryWatcherHelperService(
            ILinkerService linkerService,
            IUrlShorteningService urlShorteningService,
            ISession session,
            ILogger logger,
            ICommandParser commandParser,
            IMediaWikiApiHelper apiHelper,
            IResponder responder,
            IWatcherConfigurationService watcherConfig,
            IItemPersistenceService watcherItemPersistence)
        {
            this.linkerService = linkerService;
            this.urlShorteningService = urlShorteningService;
            this.session = session;
            this.logger = logger;
            this.apiHelper = apiHelper;
            this.responder = responder;
            this.watcherConfig = watcherConfig;
            this.watcherItemPersistence = watcherItemPersistence;

            logger.DebugFormat("Registering CategoryWatcher keys in CommandParser");
            foreach (var category in this.watcherConfig.GetWatchers())
            {
                commandParser.RegisterCommand(category.Keyword, typeof(ForceUpdateCommand));
            }
        }
        
        #region Legacy stuff
        
        [Obsolete]
        public string ConstructDefaultMessage(
            CategoryWatcher category,
            CategoryWatcherChannel categoryChannel,
            IReadOnlyCollection<CategoryWatcherItem> items,
            bool describeNewItems,
            bool describeEmptySet)
        {
            var destination = categoryChannel.Channel.Name;
            var categoryKeyword = category.Keyword;
            var showItemLinks = categoryChannel.ShowLink;
            var showWaitTime = categoryChannel.ShowWaitTime;
            var categoryChannelMinWaitTime = categoryChannel.MinWaitTime;
            
            return this.ConstructResultMessage(items, categoryKeyword, destination, describeNewItems, describeEmptySet, showItemLinks, showWaitTime, categoryChannelMinWaitTime);
        }

        /// <summary>
        /// Takes a category, and returns the added/removed items for that category, updating the category in the process
        /// </summary>
        /// <param name="category"></param>
        /// <returns>Tuple of (added, removed)</returns>
        public Tuple<List<CategoryWatcherItem>, List<CategoryWatcherItem>> UpdateCategoryItems(CategoryWatcher category)
        {
            var added = new List<CategoryWatcherItem>();
            List<CategoryWatcherItem> removed;

            // fetch category information    
            List<string> pagesInCategory;
            
            var mediaWikiApi = this.apiHelper.GetApi(category.BaseWikiId);
            try
            {
                pagesInCategory = mediaWikiApi.GetPagesInCategory(category.Category).ToList();
                pagesInCategory.RemoveAll(x => this.watcherItemPersistence.GetIgnoredPages().Contains(x));

                CategoryWatcherCount.WithLabels(category.Keyword).Set(pagesInCategory.Count);
            }
            catch (Exception e)
            {
                this.logger.WarnFormat(e, "Exception while retrieving category information for {0}", category.Keyword);
                throw;
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }
            
            // update categoryinto database
            lock (this.session)
            {
                ITransaction txn = null;

                try
                {
                    txn = this.session.BeginTransaction();

                    // reload the category's info
                    this.session.Refresh(category);

                    removed = category.CategoryItems.Where(x => !pagesInCategory.Contains(x.Title)).ToList();
                    var addedTitles = pagesInCategory.Where(x => category.CategoryItems.All(y => y.Title != x))
                        .ToList();

                    foreach (var item in removed)
                    {
                        category.CategoryItems.Remove(item);
                    }

                    foreach (var item in addedTitles)
                    {
                        var catItem = new CategoryWatcherItem
                        {
                            InsertTime = DateTime.Now,
                            Title = item,
                            Watcher = category
                        };

                        added.Add(catItem);
                        category.CategoryItems.Add(catItem);
                    }

                    this.session.Update(category);

                    txn.Commit();
                }
                catch (Exception ex)
                {
                    this.logger.ErrorFormat(
                        ex,
                        "Error encountered during catwatcher database txn for {0}",
                        category.Keyword);
                    throw;
                }
                finally
                {
                    if (txn != null && txn.IsActive)
                    {
                        txn.Rollback();
                    }
                }
            }

            var result = new Tuple<List<CategoryWatcherItem>, List<CategoryWatcherItem>>(added, removed);
            return result;
        }
        #endregion
        
        public static (List<string> added, List<string> removed) CalculateListDelta(
            List<string> originalItems,
            List<string> newItems)
        {
            var removed = originalItems.Except(newItems).ToList();
            var added = newItems.Except(originalItems).ToList();
            
            return (added, removed);
        }

        public (List<CategoryWatcherItem> allItems, IList<CategoryWatcherItem> addedItems, List<CategoryWatcherItem> removedItems) SyncItemsToDatabase(List<string> currentTitles, int watcherId)
        {
            var persistedItems = this.watcherItemPersistence.GetItems(watcherId).ToList();

            var (added, removed) = CalculateListDelta(persistedItems.Select(x => x.Title).ToList(), currentTitles);
            
            var addedItems = this.watcherItemPersistence.AddNewItems(watcherId, added);
            var removedItems = removed.Select(x => persistedItems.FirstOrDefault(y => y.Title == x)).ToList();

            this.watcherItemPersistence.RemoveDeletedItems(watcherId, removed);
            
            persistedItems = persistedItems.Where(x => !removed.Contains(x.Title)).ToList();
            persistedItems.AddRange(addedItems);

            return (persistedItems, addedItems, removedItems);
        }

        public IList<string> FetchCategoryItems(int watcherId)
        {
            var watcher = this.watcherConfig.GetWatchers().FirstOrDefault(x => x.Id == watcherId);
            return this.FetchCategoryItemsInternal(watcher);
        }

        public IList<string> FetchCategoryItems(string keyword)
        {
            var watcher = this.watcherConfig.GetWatchers().FirstOrDefault(x => x.Keyword == keyword);
            return this.FetchCategoryItemsInternal(watcher);
        }

        private IList<string> FetchCategoryItemsInternal(CategoryWatcher watcher)
        {
            List<string> pagesInCategory;

            var mediaWikiApi = this.apiHelper.GetApi(watcher.BaseWikiId);
            try
            {
                pagesInCategory = mediaWikiApi.GetPagesInCategory(watcher.Category).ToList();
                pagesInCategory.RemoveAll(x => this.watcherItemPersistence.GetIgnoredPages().Contains(x));

                CategoryWatcherCount.WithLabels(watcher.Keyword).Set(pagesInCategory.Count);
            }
            catch (Exception e)
            {
                this.logger.WarnFormat(e, "Exception while retrieving category information for {0}", watcher.Keyword);
                throw;
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }

            return pagesInCategory;
        }

        public string GetMessagePart(string watcherKey, string messageKey, string context, object[] arguments = null, Context contextType = null)
        {
            var fullMessageKey = $"catwatcher.item.{watcherKey}.{messageKey}";
            var defaultMessageKey = $"catwatcher.item.default.{messageKey}";

            var response = this.responder.GetMessagePart(fullMessageKey, context, arguments, contextType);

            if (response == null)
            {
                response = this.responder.GetMessagePart(defaultMessageKey, context, arguments, contextType);
            }

            return response;
        }
        
        public string GetMessagePart(string watcherKey, string messageKey, string context, object argument)
        {
            return this.GetMessagePart(watcherKey, messageKey, context, new[] { argument });
        }
        
        public string ConstructResultMessage(
            IReadOnlyCollection<CategoryWatcherItem> items,
            string categoryKeyword,
            string destination,
            bool describeNewItems,
            bool describeEmptySet,
            bool showItemLinks,
            bool showWaitTime,
            int categoryChannelMinWaitTime)
        {
            var pluralString = this.GetMessagePart(categoryKeyword, "plural", destination);

            if (items.Any())
            {
                var textItems = new List<string>();

                foreach (var item in items)
                {
                    // Display an http URL to the page, if desired
                    var urlData = string.Empty;
                    if (showItemLinks)
                    {
                        var pageUrl = this.linkerService.ConvertWikilinkToUrl(destination, item.Title);
                        urlData = " " + this.urlShorteningService.Shorten(pageUrl);
                    }

                    // show the waiting time for the request
                    var waitTimeData = string.Empty;

                    if (showWaitTime)
                    {
                        var waitingTime = DateTime.UtcNow - item.InsertTime;

                        if (waitingTime >= new TimeSpan(0, 0, 0, categoryChannelMinWaitTime))
                        {
                            var waitTimeFormat = string.Format(
                                " (waiting {{0:{0}hh\\:mm}})",
                                waitingTime.TotalDays > 1 ? "d\\d\\ " : string.Empty);

                            waitTimeData = string.Format(waitTimeFormat, waitingTime);
                        }
                    }

                    textItems.Add(string.Format("[[{0}]]{1}{2}", item.Title, urlData, waitTimeData));
                }

                if (items.Count == 1)
                {
                    pluralString = this.GetMessagePart(categoryKeyword, "singular", destination);
                }

                object[] messageParams =
                {
                    items.Count,
                    pluralString,
                    string.Join(" , ", textItems)
                };

                var keySuffix = "hasitems";
                if (describeNewItems)
                {
                    keySuffix = "newitems";
                }

                return this.GetMessagePart(categoryKeyword, keySuffix, destination, messageParams);
            }

            if (!describeEmptySet)
            {
                return null;
            }

            return this.GetMessagePart(categoryKeyword, "noitems", destination, pluralString);
        }
    }
}