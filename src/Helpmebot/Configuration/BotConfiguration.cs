namespace Helpmebot.Configuration
{
    using System;

    public class BotConfiguration
    {
        public BotConfiguration(string commandTrigger,
            string userAgent,
            int httpTimeout,
            string debugChannel,
            bool enableNotificationService)
        {
            if (commandTrigger == null)
            {
                throw new ArgumentNullException("commandTrigger");
            }

            if (userAgent == null)
            {
                throw new ArgumentNullException("userAgent");
            }

            if (debugChannel == null)
            {
                throw new ArgumentNullException("debugChannel");
            }

            this.CommandTrigger = commandTrigger;
            this.UserAgent = userAgent;
            this.HttpTimeout = httpTimeout;
            this.DebugChannel = debugChannel;
            this.EnableNotificationService = enableNotificationService;
        }

        public string CommandTrigger { get; private set; }
        public string UserAgent { get; private set; }
        public int HttpTimeout { get; private set; }
        public string DebugChannel { get; private set; }
        public bool EnableNotificationService { get; private set; }
        public string GoogleApiKey { get; set; }
        public string IpInfoDbApiKey { get; set; }
        public string MaxMindDatabasePath { get; set; }
        public int? SystemMonitoringPort { get; set; }
    }
}