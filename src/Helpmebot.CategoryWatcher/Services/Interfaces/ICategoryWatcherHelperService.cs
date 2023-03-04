namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System.Collections.Generic;
    using Helpmebot.Model;

    public interface ICategoryWatcherHelperService
    {
        (IList<CategoryWatcherItem> allItems, IList<CategoryWatcherItem> addedItems, IList<CategoryWatcherItem> removedItems) SyncCategoryItems(string keyword);

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
    }
}