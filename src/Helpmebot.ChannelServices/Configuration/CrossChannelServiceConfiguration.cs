namespace Helpmebot.ChannelServices.Configuration
{
    public class CrossChannelServiceConfiguration
    {
        public CrossChannelServiceConfiguration(int rateLimitMax, int rateLimitDuration)
        {
            this.RateLimitMax = rateLimitMax;
            this.RateLimitDuration = rateLimitDuration;
        }

        public int RateLimitMax { get; private set; }
        public int RateLimitDuration { get; private set; }
    }
}