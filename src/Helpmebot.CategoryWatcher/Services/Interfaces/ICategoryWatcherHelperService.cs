namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Helpmebot.Model;

    public interface ICategoryWatcherHelperService
    {
        string ConstructDefaultMessage(
            CategoryWatcher category,
            CategoryWatcherChannel categoryChannel,
            IReadOnlyCollection<CategoryWatcherItem> items,
            bool isNew,
            bool describeEmptySet);

        /// <summary>
        /// Takes a category, and returns the added/removed items for that category, updating the category in the process
        /// </summary>
        /// <param name="category"></param>
        /// <returns>Tuple of (added, removed)</returns>
        Tuple<List<CategoryWatcherItem>, List<CategoryWatcherItem>> UpdateCategoryItems(CategoryWatcher category);

        IEnumerable<string> GetValidWatcherKeys { get; }
        IReadOnlyList<CategoryWatcher> WatchedCategories { get; }
    }
}