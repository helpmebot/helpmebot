namespace Helpmebot.Background.Interfaces
{
    using System.Collections.Generic;
    using Helpmebot.Model;

    public interface ICategoryWatcherBackgroundService : ITimerBackgroundService
    {
        void ForceUpdate(string key, Channel destination);
        IEnumerable<string> GetWatchedCategories(Channel destination);
        IEnumerable<string> GetValidWatcherKeys();
    }
}