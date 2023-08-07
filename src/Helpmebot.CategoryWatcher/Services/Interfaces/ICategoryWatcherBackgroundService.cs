namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Model;

    public interface ICategoryWatcherBackgroundService : ITimerBackgroundService
    {
        void Suspend();
        void Resume();
        void ResetChannelTimer(CategoryWatcherChannel config);
    }
}