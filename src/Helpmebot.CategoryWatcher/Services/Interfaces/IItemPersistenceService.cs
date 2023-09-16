namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System.Collections.Generic;
    using Model;

    public interface IItemPersistenceService
    {
        IList<CategoryWatcherItem> GetItems(string watcherKeyword);
        IList<CategoryWatcherItem> AddNewItems(string watcherKeyword, IEnumerable<string> newItems);
        void RemoveDeletedItems(string watcherKeyword, IEnumerable<string> deletedItems);

        IList<string> GetIgnoredPages();
        void AddIgnoredPage(string page);
        bool RemoveIgnoredPage(string page);
        IList<CategoryWatcherItem> PageIsTracked(string page);
    }
}