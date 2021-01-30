namespace Helpmebot.Configuration
{
    using System.Collections.Generic;

    public class HelpeeManagementConfiguration
    {
        public string TargetChannel { get; }
        public string MonitorChannel { get; }
        public List<string> IgnoredNicknames { get; }

        public HelpeeManagementConfiguration(string targetChannel, string monitorChannel, List<string> ignoredNicknames)
        {
            this.TargetChannel = targetChannel;
            this.MonitorChannel = monitorChannel;
            this.IgnoredNicknames = ignoredNicknames;
        }
    }
}