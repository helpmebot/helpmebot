namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Helpmebot.Model;

    public interface IWatcherConfigurationService
    {
        IReadOnlyList<CategoryWatcher> GetWatchers();
        CategoryWatcher CreateWatcher(string category, string keyword, string baseWiki);
        IEnumerable<string> GetValidWatcherKeys();
        IEnumerable<string> GetWatchersForChannel(string channelName);
        IEnumerable<string> GetChannelsForWatcher(string keyword);
        CategoryWatcherChannel GetWatcherConfiguration(string keyword, string channel, bool defaultIfUnconfigured = false);
        void DeleteWatcher(string keyword);
        void SaveWatcherConfiguration(CategoryWatcherChannel config);
        void DeleteWatcherConfiguration(string flag);
        void TouchWatcherLastSyncTime(CategoryWatcher watcher);
        event EventHandler WatcherConfigurationChanged;
    }
}