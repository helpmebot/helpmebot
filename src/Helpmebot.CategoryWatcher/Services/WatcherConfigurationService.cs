namespace Helpmebot.CategoryWatcher.Services
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using Interfaces;
    using Model;
    using NHibernate;

    public class WatcherConfigurationService : IWatcherConfigurationService
    {
        private readonly ISession databaseSession;
        private readonly ILogger logger;
        private readonly IList<CategoryWatcher> watchedCategories;
        private readonly IList<CategoryWatcherChannel> channels;

        public WatcherConfigurationService(ISession databaseSession, ILogger logger)
        {
            this.databaseSession = databaseSession;
            this.logger = logger;

            this.watchedCategories = this.databaseSession.CreateCriteria<CategoryWatcher>().List<CategoryWatcher>();
            this.channels = this.databaseSession.CreateCriteria<CategoryWatcherChannel>().List<CategoryWatcherChannel>();
        }
        
        public IEnumerable<string> GetValidWatcherKeys()
        {
             return this.watchedCategories.Select(x => x.Keyword); 
        }

        public IReadOnlyList<CategoryWatcher> GetWatchers()
        {
            return new List<CategoryWatcher>(this.watchedCategories);
        }

        public void CreateWatcher(string category, string keyword, string baseWiki)
        {
            if (this.watchedCategories.Any(x => x.Category == category && x.BaseWikiId == baseWiki))
            {
                throw new DuplicateNameException("A category watcher for this category already exists.");
            }

            var watcher = new CategoryWatcher
            {
                BaseWikiId = baseWiki,
                Category = category,
                Keyword = keyword
            };

            using (var txn = this.databaseSession.BeginTransaction())
            {
                this.databaseSession.Save(watcher);
                txn.Commit();
            }
            
            this.watchedCategories.Add(watcher);
        }
    }
}