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
    using Prometheus;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    
    using ItemList = System.Collections.Generic.IList<Model.CategoryWatcherItem>;

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
        private readonly ILogger logger;
        private readonly ICommandParser commandParser;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IWatcherConfigurationService watcherConfig;
        private readonly IItemPersistenceService watcherItemPersistence;

        public CategoryWatcherHelperService(
            ILinkerService linkerService,
            IUrlShorteningService urlShorteningService,
            ILogger logger,
            ICommandParser commandParser,
            IMediaWikiApiHelper apiHelper,
            IResponder responder,
            IWatcherConfigurationService watcherConfig,
            IItemPersistenceService watcherItemPersistence,
            ICategoryWatcherMessageService messageService,
            IResponseManager responseManager)
        {
            this.linkerService = linkerService;
            this.urlShorteningService = urlShorteningService;
            this.logger = logger;
            this.commandParser = commandParser;
            this.apiHelper = apiHelper;
            this.responder = responder;
            this.watcherConfig = watcherConfig;
            this.watcherItemPersistence = watcherItemPersistence;

            responseManager.RegisterRepository(messageService);
            
            logger.DebugFormat("Registering CategoryWatcher keys in CommandParser");
            foreach (var category in this.watcherConfig.GetWatchers())
            {
                commandParser.RegisterCommand(category.Keyword, typeof(ForceUpdateCommand));
            }
        }
        
        internal static (IList<string> added, IList<string> removed) CalculateListDelta(
            IList<string> originalItems,
            IList<string> newItems)
        {
            var removed = originalItems.Except(newItems).ToList();
            var added = newItems.Except(originalItems).ToList();
            
            return (added, removed);
        }

        public (ItemList allItems, ItemList addedItems, ItemList removedItems) SyncCategoryItems(string keyword, bool doSync = true)
        {
            var watcher = this.watcherConfig.GetWatchers().FirstOrDefault(x => x.Keyword == keyword);
            if (watcher == null)
            {
                throw new ArgumentOutOfRangeException(nameof(keyword));
            }

            if (!doSync)
            {
                return (this.watcherItemPersistence.GetItems(keyword).ToList(), new List<CategoryWatcherItem>(),
                    new List<CategoryWatcherItem>());
            }
            
            var items = this.FetchCategoryItemsInternal(watcher);
            this.watcherConfig.TouchWatcherLastSyncTime(watcher);
            
            return SyncItemsToDatabase(items, watcher.Keyword);
        }
        
        internal (ItemList allItems, ItemList addedItems, ItemList removedItems) SyncItemsToDatabase(IList<string> currentTitles, string watcherKeyword)
        {
            var persistedItems = this.watcherItemPersistence.GetItems(watcherKeyword).ToList();

            var (added, removed) = CalculateListDelta(persistedItems.Select(x => x.Title).ToList(), currentTitles);
            
            var addedItems = this.watcherItemPersistence.AddNewItems(watcherKeyword, added);
            var removedItems = removed.Select(x => persistedItems.FirstOrDefault(y => y.Title == x)).ToList();

            this.watcherItemPersistence.RemoveDeletedItems(watcherKeyword, removed);
            
            persistedItems = persistedItems.Where(x => !removed.Contains(x.Title)).ToList();
            persistedItems.AddRange(addedItems);

            return (persistedItems, addedItems, removedItems);
        }

        [Obsolete("Only used by tests")]
        internal IList<string> FetchCategoryItems(string keyword)
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

        internal void TouchCategoryItemsMetric(string keyword, bool increment)
        {
            if (increment)
            {
                CategoryWatcherCount.WithLabels(keyword).Inc();
            }
            else
            {
                CategoryWatcherCount.WithLabels(keyword).Dec();
            }
        }
        
        private string GetMessagePart(string watcherKey, string messageKey, string context, object[] arguments = null, Context contextType = null)
        {
            var fullMessageKey = $"catwatcher.item.{watcherKey}.{messageKey}";
            return this.responder.GetMessagePart(fullMessageKey, context, arguments, contextType);
        }
        
        internal string GetMessagePart(string watcherKey, string messageKey, string context, object argument)
        {
            return this.GetMessagePart(watcherKey, messageKey, context, new[] { argument });
        }
        
        public string ConstructResultMessage(
            IList<CategoryWatcherItem> items,
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


        public string ConstructRemovalMessage(
            IList<CategoryWatcherItem> removed,
            string categoryKeyword,
            string destination
        )
        {               
            var removalList = string.Join(", ", removed.Select(x => $"[[{x.Title}]]"));
            var message = this.GetMessagePart(categoryKeyword, "handled", destination, removalList);

            return message;
        }

        public void RegisterWatcher(CategoryWatcher watcher)
        {
            this.commandParser.RegisterCommand(watcher.Keyword, typeof(ForceUpdateCommand));
        }

        public void DeregisterWatcher(string keyword)
        {
            if (this.watcherConfig.GetWatchers().Any(x => x.Keyword == keyword))
            {
                this.commandParser.UnregisterCommand(keyword);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(keyword));
            }
        }
    }
}