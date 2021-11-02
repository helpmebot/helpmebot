// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Helpmebot.ChannelServices.Configuration
{
    using System.Collections.Generic;

    public class HelpeeManagementConfiguration
    {
        public string TargetChannel { get; set; }
        public string MonitorChannel { get; set; }
        public List<string> IgnoredNicknames { get; set; }
    }
}