namespace Helpmebot.CategoryWatcher.Configuration
{
    public class CategoryWatcherConfiguration
    {
        public bool Enabled { get; set; }
        public int MinReportFrequency { get; set; }
        public int SyncFrequency { get; set; }
        public int CrossoverTimeout { get; set; }
        public bool UseMq { get; set; }
    }
}