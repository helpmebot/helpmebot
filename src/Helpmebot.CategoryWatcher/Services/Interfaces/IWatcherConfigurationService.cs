namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System.Collections.Generic;
    using Model;

    public interface IWatcherConfigurationService
    {
        IReadOnlyList<CategoryWatcher> GetWatchers();
        void CreateWatcher(string category, string keyword, string baseWiki);
        IEnumerable<string> GetValidWatcherKeys();
    }
}