// ReSharper disable UnusedAutoPropertyAccessor.Global - used by YAML deserialize
namespace Helpmebot.Configuration
{
    using System;

    public class BotConfiguration
    {
        public string CommandTrigger { get; set; }
        public string UserAgent { get; set; }
        public string DebugChannel { get; set; }
        public bool DisableCertificateValidation { get; set; }
        public string IpInfoDbApiKey { get; set; }
        public string MaxMindDatabasePath { get; set; }
        public int? PrometheusMetricsPort { get; set; }
        public string Log4NetConfiguration { get; set; }
    }
}