namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Commands.CategoryMonitoring;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
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
        private readonly IMessageService messageService;
        private readonly ISession session;
        private readonly ILogger logger;
        private readonly IMediaWikiApiHelper apiHelper;

        private readonly IList<WatchedCategory> watchedCategories;
        private readonly IList<string> ignoredPages;

        public CategoryWatcherHelperService(
            ILinkerService linkerService,
            IUrlShorteningService urlShorteningService,
            IMessageService messageService,
            ISession session,
            ILogger logger,
            ICommandParser commandParser,
            IMediaWikiApiHelper apiHelper)
        {
            this.linkerService = linkerService;
            this.urlShorteningService = urlShorteningService;
            this.messageService = messageService;
            this.session = session;
            this.logger = logger;
            this.apiHelper = apiHelper;

            lock (this.session)
            {
                this.watchedCategories = this.session.CreateCriteria<WatchedCategory>().List<WatchedCategory>();

                this.ignoredPages = this.session.CreateCriteria<IgnoredPage>()
                    .List<IgnoredPage>()
                    .Select(x => x.Title)
                    .ToList();
            }

            logger.DebugFormat("Registering CategoryWatcher keys in CommandParser");
            foreach (var category in this.watchedCategories)
            {
                commandParser.RegisterCommand(category.Keyword, typeof(ForceUpdateCommand));
            }
        }

        public IEnumerable<string> GetValidWatcherKeys
        {
            get { return this.watchedCategories.Select(x => x.Keyword); }
        }

        public IReadOnlyList<WatchedCategory> WatchedCategories
        {
            get { return new List<WatchedCategory>(this.watchedCategories); }
        }

        public string ConstructDefaultMessage(
            WatchedCategory category,
            CategoryWatcherChannel categoryChannel,
            IReadOnlyCollection<CategoryItem> items,
            bool isNew,
            bool describeEmptySet)
        {
            var destination = categoryChannel.Channel.Name;

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

                string pluralString;
                if (items.Count == 1)
                {
                    pluralString = this.messageService.RetrieveMessage(
                        category.Keyword + "Singular",
                        destination,
                        new[] {"keywordSingularDefault"});
                }
                else
                {
                    pluralString = this.messageService.RetrieveMessage(
                        category.Keyword + "Plural",
                        destination,
                        new[] {"keywordPluralDefault"});
                }

                string[] messageParams =
                {
                    items.Count.ToString(CultureInfo.InvariantCulture), pluralString,
                    string.Join(" , ", textItems)
                };

                return this.messageService.RetrieveMessage(
                    category.Keyword + (isNew ? "New" : string.Empty) + "HasItems",
                    destination,
                    messageParams);
            }

            if (!describeEmptySet)
            {
                return null;
            }

            string[] mp =
            {
                this.messageService.RetrieveMessage(
                    category.Keyword + "Plural",
                    destination,
                    new[] {"keywordPluralDefault"})
            };
            return this.messageService.RetrieveMessage(category.Keyword + "NoItems", destination, mp);
        }

        /// <summary>
        /// Takes a category, and returns the added/removed items for that category, updating the category in the process
        /// </summary>
        /// <param name="category"></param>
        /// <returns>Tuple of (added, removed)</returns>
        public Tuple<List<CategoryItem>, List<CategoryItem>> UpdateCategoryItems(WatchedCategory category)
        {
            var added = new List<CategoryItem>();
            List<CategoryItem> removed;

            // fetch category information    
            List<string> pagesInCategory;

            
            var mediaWikiApi = this.apiHelper.GetApi(category.BaseWiki);
            try
            {
                pagesInCategory = mediaWikiApi.GetPagesInCategory(category.Category).ToList();
                pagesInCategory.RemoveAll(x => this.ignoredPages.Contains(x));

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
                        var catItem = new CategoryItem
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

            var result = new Tuple<List<CategoryItem>, List<CategoryItem>>(added, removed);
            return result;
        }
    }
}