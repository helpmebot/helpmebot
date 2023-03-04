namespace Helpmebot.CategoryWatcher.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using CoreServices.Services.Interfaces;
    using Interfaces;
    using Model;
    using NHibernate;

    public class WatcherConfigurationService : IWatcherConfigurationService
    {
        private readonly ISession databaseSession;
        private readonly ILogger logger;
        private readonly IChannelManagementService channelManagementService;
        private readonly IList<CategoryWatcher> watchedCategories;
        private readonly IList<CategoryWatcherChannel> channels;

        public WatcherConfigurationService(ISession databaseSession, ILogger logger, IChannelManagementService channelManagementService)
        {
            this.databaseSession = databaseSession;
            this.logger = logger;
            this.channelManagementService = channelManagementService;

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

        public IEnumerable<string> GetWatchersForChannel(string channelName)
        {
            // FIXME: remove temp hack
            var id = this.channelManagementService.GetIdFromName(channelName);
            
            return this.channels
                .Where(x => id != null && x.ChannelId == id)
                .Select(x => x.Watcher.Keyword);
        }

        public IEnumerable<string> GetChannelsForWatcher(string keyword)
        {
            var watcher = this.watchedCategories.SingleOrDefault(x => x.Keyword == keyword);

            if (watcher == null)
            {
                return Array.Empty<string>();
            }

            return this.channels.Where(x => x.Watcher.Id == watcher.Id).Select(x => this.channelManagementService.GetNameFromId(x.ChannelId)).ToList();
        }

        public CategoryWatcherChannel GetWatcherConfiguration(string keyword, string channel)
        {
            var watcher = this.watchedCategories.SingleOrDefault(x => x.Keyword == keyword);
            var channelId = this.channelManagementService.GetIdFromName(channel);
            
            if (watcher == null)
            {
                throw new ArgumentOutOfRangeException(nameof(keyword));
            }

            if (channelId == null)
            {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }

            return this.channels.SingleOrDefault(x => x.Watcher.Id == watcher.Id && x.ChannelId == channelId);
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