namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System.Collections.Generic;
    using Model;

    public interface IItemPersistenceService
    {
        IList<CategoryWatcherItem> GetItems(int watcherId);
        IList<CategoryWatcherItem> AddNewItems(int watcherId, IEnumerable<string> newItems);
        void RemoveDeletedItems(int watcherId, IEnumerable<string> deletedItems);

        IList<string> GetIgnoredPages();
        void AddIgnoredPage(string page);
        void RemoveIgnoredPage(string page);
    }
}