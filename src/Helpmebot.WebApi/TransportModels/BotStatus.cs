namespace Helpmebot.WebApi.TransportModels
{
    using System;
    using System.Collections.Generic;

    public class BotStatus
    {
        public string Nickname { get; set; }
        public string IrcServer { get; set; }
        public int IrcServerPort { get; set; }
        public List<string> Commands { get; set; }
        public int ChannelCount { get; set; }
        public int VisibleUserCount { get; set; }
        
        public double PingTime { get; set; }

        public int TotalMessages { get; set; }
        public string Trigger { get; set; }
        public DateTime StartupTime { get; set; }
    }
}