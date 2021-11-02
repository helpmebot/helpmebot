// ReSharper disable UnusedAutoPropertyAccessor.Global - used by YAML deserialize
namespace Helpmebot.Configuration
{
    using System;

    public class BotConfiguration
    {
        public string CommandTrigger { get; set; }
        public string UserAgent { get; set; }
        public int HttpTimeout { get; set; }
        public string DebugChannel { get; set; }
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
    }
}