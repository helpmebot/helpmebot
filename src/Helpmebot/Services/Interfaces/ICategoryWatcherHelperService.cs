namespace Helpmebot.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Helpmebot.Model;

    public interface ICategoryWatcherHelperService
    {
        string ConstructDefaultMessage(
            WatchedCategory category,
            CategoryWatcherChannel categoryChannel,
            IReadOnlyCollection<CategoryItem> items,
            bool isNew,
            bool describeEmptySet);

        /// <summary>
        /// Takes a category, and returns the added/removed items for that category, updating the category in the process
        /// </summary>
        /// <param name="category"></param>
        /// <returns>Tuple of (added, removed)</returns>
        Tuple<List<CategoryItem>, List<CategoryItem>> UpdateCategoryItems(WatchedCategory category);

        IEnumerable<string> GetValidWatcherKeys { get; }
        IReadOnlyList<WatchedCategory> WatchedCategories { get; }
    }
}