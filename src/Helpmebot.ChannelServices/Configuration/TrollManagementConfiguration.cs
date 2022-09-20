namespace Helpmebot.ChannelServices.Configuration
{
    using System.Collections.Generic;

    public class TrollManagementConfiguration
    {
        public string TargetChannel { get; set; }
        public string PublicAlertChannel { get; set; }
        public List<string> PrivateAlertTargets { get; set; }
        public string BanTracker { get; set; }
        public string AntiSpamBot { get; set; }
        public string OpTargetAccount { get; set; }
        
        public string BadWordRegex { get; set; }
        public string ReallyBadWordRegex { get; set; }
        public string InstaQuietRegex { get; set; }
        public string FirstMessageQuietRegex { get; set; }
        public string PasteRegex { get; set; }
    }
}