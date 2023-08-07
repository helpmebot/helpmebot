namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System.Collections.Generic;
    using Helpmebot.Model;
    using ItemList = System.Collections.Generic.IList<Model.CategoryWatcherItem>;

    public interface ICategoryWatcherHelperService
    {
        (ItemList allItems, ItemList addedItems, ItemList removedItems) SyncCategoryItems(string keyword, bool doSync = true);

        string ConstructResultMessage(
            IList<CategoryWatcherItem> items,
            string categoryKeyword,
            string destination,
            bool describeNewItems,
            bool describeEmptySet,
            bool showItemLinks,
            bool showWaitTime,
            int categoryChannelMinWaitTime);

        string ConstructRemovalMessage(
            IList<CategoryWatcherItem> removed,
            string categoryKeyword,
            string destination
        );

        void RegisterWatcher(CategoryWatcher watcher);
        void DeregisterWatcher(string keyword);
    }
}