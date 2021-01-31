namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System.Collections.Generic;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Model;

    public interface ICategoryWatcherBackgroundService : ITimerBackgroundService
    {
        void ForceUpdate(string key, Channel destination);
        IEnumerable<string> GetWatchedCategories(Channel destination);
        IEnumerable<string> GetValidWatcherKeys();
    }
}