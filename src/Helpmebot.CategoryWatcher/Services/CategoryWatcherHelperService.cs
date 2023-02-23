namespace Helpmebot.CategoryWatcher.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
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
        


        public string ConstructDefaultMessage(
            CategoryWatcher category,
            CategoryWatcherChannel categoryChannel,
            IReadOnlyCollection<CategoryWatcherItem> items,
            bool isNew,
            bool describeEmptySet)
        {
            var destination = categoryChannel.Channel.Name;
            var pluralString = this.responder.GetMessagePart(
                $"catwatcher.item.{category.Keyword}.plural",
                destination);

            if (pluralString == null)
            {
                pluralString = this.responder.GetMessagePart(
                    $"catwatcher.item.default.plural",
                    destination);
            }
            
            if (items.Any())
            {
                var textItems = new List<string>();

                foreach (var item in items)
                {
                    // Display an http URL to the page, if desired
                    var urlData = string.Empty;
                    if (categoryChannel.ShowLink)
                    {
                        var pageUrl = this.linkerService.ConvertWikilinkToUrl(destination, item.Title);
                        urlData = " " + this.urlShorteningService.Shorten(pageUrl);
                    }

                    // show the waiting time for the request
                    var waitTimeData = string.Empty;
                    if (categoryChannel.ShowWaitTime)
                    {
                        var waitingTime = DateTime.Now - item.InsertTime;

                        if (waitingTime >= new TimeSpan(0, 0, 0, categoryChannel.MinWaitTime))
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
                    pluralString = this.responder.GetMessagePart(
                        $"catwatcher.item.{category.Keyword}.singular",
                        destination);

                    if (pluralString == null)
                    {
                        pluralString = this.responder.GetMessagePart(
                            $"catwatcher.item.default.singular",
                            destination);
                    }
                }

                object[] messageParams =
                {
                    items.Count,
                    pluralString,
                    string.Join(" , ", textItems)
                };

                var keySuffix = "hasitems";
                if (isNew)
                {
                    keySuffix = "newitems";
                }
                
                var message = this.responder.GetMessagePart(
                    $"catwatcher.item.{category.Keyword}.{keySuffix}",
                    destination, messageParams);

                if (message == null)
                {
                    message = this.responder.GetMessagePart(
                        $"catwatcher.item.default.{keySuffix}",
                        destination, messageParams);
                }

                return message;
            }

            if (!describeEmptySet)
            {
                return null;
            }

            var noitems = this.responder.GetMessagePart(
                $"catwatcher.item.{category.Keyword}.noitems",
                destination,
                pluralString
            );

            if (noitems == null)
            {
                noitems = this.responder.GetMessagePart(
                    $"catwatcher.item.default.noitems",
                    destination, pluralString);
            }

            return noitems;
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
    }
}