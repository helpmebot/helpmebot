namespace Helpmebot.Model
{
    using Helpmebot.Persistence;

    public class CategoryWatcherChannel : EntityBase
    {
        public virtual string Channel { get; set; }
        public virtual string Watcher { get; set; }
        public virtual int SleepTime { get; set; }
        public virtual bool ShowWaitTime { get; set; }
        public virtual bool ShowLink { get; set; }
        public virtual bool AlertForAdditions { get; set; }
        public virtual bool AlertForRemovals { get; set; }
        public virtual int MinWaitTime { get; set; }
        public virtual bool Enabled { get; set; }
    }
}