namespace Helpmebot.Configuration
{
    using System;

    public class BotConfiguration
    {
        public BotConfiguration(string commandTrigger,
            string userAgent,
            int httpTimeout,
            string debugChannel
        )
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
        }

        public string CommandTrigger { get; private set; }
        public string UserAgent { get; private set; }
        public int HttpTimeout { get; private set; }
        public string DebugChannel { get; private set; }
        [Obsolete]
        public bool EnableNotificationService { get; set; }
        public bool DisableCertificateValidation { get; set; }
        [Obsolete]
        public string GoogleApiKey { get; set; }
        public string IpInfoDbApiKey { get; set; }
        public string MaxMindDatabasePath { get; set; }
        [Obsolete]
        public string AccDeploymentPassword { get; set; }
        [Obsolete]
        public int? SystemMonitoringPort { get; set; }
        public int? PrometheusMetricsPort { get; set; }
        public string Log4NetConfiguration { get; set; }
        
        public string ModuleConfigurationPath { get; set; }
    }
}