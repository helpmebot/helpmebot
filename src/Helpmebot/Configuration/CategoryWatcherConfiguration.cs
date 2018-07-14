namespace Helpmebot.Configuration
{
    public class CategoryWatcherConfiguration
    {
        public bool Enabled { get; private set; }
        public int UpdateFrequency { get; private set; }
        public int CrossoverTimeout { get; private set; }

        public CategoryWatcherConfiguration(bool enabled, int updateFrequency, int crossoverTimeout)
        {
            this.Enabled = enabled;
            this.UpdateFrequency = updateFrequency;
            this.CrossoverTimeout = crossoverTimeout;
        }   
    }
}