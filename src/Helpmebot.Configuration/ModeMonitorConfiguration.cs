namespace Helpmebot.Configuration
{
    using System.Collections.Generic;

    public class ModeMonitorConfiguration
    {
        public ModeMonitorConfiguration(IDictionary<string, string> channelMap)
        {
            this.ChannelMap = channelMap;
        }

        public IDictionary<string, string> ChannelMap { get; private set; }
    }
}