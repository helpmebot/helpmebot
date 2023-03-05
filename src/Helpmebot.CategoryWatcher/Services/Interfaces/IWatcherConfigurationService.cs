namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System.Collections.Generic;
    using Model;

    public interface IWatcherConfigurationService
    {
        IReadOnlyList<CategoryWatcher> GetWatchers();
        CategoryWatcher CreateWatcher(string category, string keyword, string baseWiki);
        IEnumerable<string> GetValidWatcherKeys();
        IEnumerable<string> GetWatchersForChannel(string channelName);
        IEnumerable<string> GetChannelsForWatcher(string keyword);
        CategoryWatcherChannel GetWatcherConfiguration(string keyword, string channel);
        void DeleteWatcher(string keyword);
    }
}