namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Model;

    public interface ICategoryWatcherBackgroundService : ITimerBackgroundService
    {
        void ForceUpdate(string key, Channel destination);
    }
}