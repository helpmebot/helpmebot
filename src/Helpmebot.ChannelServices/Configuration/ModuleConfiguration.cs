// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Helpmebot.ChannelServices.Configuration
{
    using System.Collections.Generic;

    public class ModuleConfiguration
    {
        public RateLimitConfiguration CrossChannelRateLimits { get; set; }
        public RateLimitConfiguration JoinMessageRateLimits { get; set; }
        
        public HelpeeManagementConfiguration HelpeeManagement { get; set; }
        
        public TrollManagementConfiguration TrollManagement { get; set; }

        public IDictionary<string, string> ModeMonitorChannelMap { get; set; }
        
        public IDictionary<string, string> AlertOnRanges { get; set; }

    }
}