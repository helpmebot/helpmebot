namespace Helpmebot.CategoryWatcher.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Interfaces;
    using Model;
    using NHibernate;
    using NHibernate.Criterion;

    public class ItemPersistenceService : IItemPersistenceService
    {
        private readonly ISession databaseSession;
        private readonly ILogger logger;

        public ItemPersistenceService(ISession databaseSession, ILogger logger)
        {
            this.databaseSession = databaseSession;
            this.logger = logger;
        }

        public IList<CategoryWatcherItem> GetItems(string watcherKeyword)
        {
            lock (this.databaseSession)
            {
                return this.GetItemsInternal(watcherKeyword);
            }
        }

        private IList<CategoryWatcherItem> GetItemsInternal(string watcherKeyword)
        {
            return this.databaseSession.CreateCriteria<CategoryWatcherItem>()
                .Add(Restrictions.Eq(nameof(CategoryWatcherItem.Watcher), watcherKeyword))
                .List<CategoryWatcherItem>();
        }

        public IList<CategoryWatcherItem> AddNewItems(string watcherKeyword, IEnumerable<string> newItems)
        {
            lock (this.databaseSession)
            {
                var existing = this.GetItemsInternal(watcherKeyword);
                var insertTime = DateTime.UtcNow;

                var newRecords = newItems
                    .Where(x => existing.All(i => i.Title != x))
                    .Select(
                        x => new CategoryWatcherItem
                        {
                            InsertTime = insertTime,
                            Title = x,
                            Watcher = watcherKeyword
                        })
                    .ToList();
                using (var txn = this.databaseSession.BeginTransaction())
                {
                    foreach (var r in newRecords)
                    {
                        this.databaseSession.Save(r);
                    }

                    txn.Commit();
                }
                
                return newRecords;
            }
        }

        public void RemoveDeletedItems(string watcherKeyword, IEnumerable<string> deletedItems)
        {
            lock (this.databaseSession)
            {
                using (var txn = this.databaseSession.BeginTransaction())
                {
                    foreach (var itemTitle in deletedItems)
                    {
                        var obj = this.databaseSession.CreateCriteria<CategoryWatcherItem>()
                            .Add(Restrictions.Eq(nameof(CategoryWatcherItem.Title), itemTitle))
                            .Add(Restrictions.Eq(nameof(CategoryWatcherItem.Watcher), watcherKeyword))
                            .UniqueResult<CategoryWatcherItem>();

                        if (obj == null)
                        {
                            return;
                        }

                        this.databaseSession.Delete(obj);
                    }

                    txn.Commit();
                }
            }
        }

        public IList<string> GetIgnoredPages()
        {
            lock (this.databaseSession)
            {
                return this.databaseSession.CreateCriteria<CategoryWatcherIgnoredPage>()
                    .List<CategoryWatcherIgnoredPage>()
                    .Select(x => x.Title)
                    .ToList();
            }
        }

        public void AddIgnoredPage(string page)
        {
            lock (this.databaseSession)
            {
                using (var txn = this.databaseSession.BeginTransaction())
                {
                    var obj = this.databaseSession.CreateCriteria<CategoryWatcherIgnoredPage>()
                        .Add(Restrictions.Eq(nameof(CategoryWatcherIgnoredPage.Title), page))
                        .UniqueResult<CategoryWatcherIgnoredPage>();

                    if (obj != null)
                    {
                        return;
                    }

                    this.databaseSession.Save(
                        new CategoryWatcherIgnoredPage
                        {
                            Title = page
                        });

                    txn.Commit();
                }
            }
        }

        public void RemoveIgnoredPage(string page)
        {
            lock (this.databaseSession)
            {
                using (var txn = this.databaseSession.BeginTransaction())
                {
                    var obj = this.databaseSession.CreateCriteria<CategoryWatcherIgnoredPage>()
                        .Add(Restrictions.Eq(nameof(CategoryWatcherIgnoredPage.Title), page))
                        .UniqueResult<CategoryWatcherIgnoredPage>();

                    if (obj == null)
                    {
                        return;
                    }

                    this.databaseSession.Delete(obj);
                    txn.Commit();
                }
            }
        }
    }
}