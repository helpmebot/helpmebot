namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Helpmebot.Model;

    public interface ICategoryWatcherHelperService
    {
        /// <summary>
        /// Takes a category, and returns the added/removed items for that category, updating the category in the process
        /// </summary>
        /// <param name="category"></param>
        /// <returns>Tuple of (added, removed)</returns>
        Tuple<List<CategoryWatcherItem>, List<CategoryWatcherItem>> UpdateCategoryItems(CategoryWatcher category);

        string ConstructResultMessage(
            IReadOnlyCollection<CategoryWatcherItem> items,
            string categoryKeyword,
            string destination,
            bool describeNewItems,
            bool describeEmptySet,
            bool showItemLinks,
            bool showWaitTime,
            int categoryChannelMinWaitTime);
    }
}