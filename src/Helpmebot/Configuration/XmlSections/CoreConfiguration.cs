namespace Helpmebot.Configuration.XmlSections
{
    using System.Configuration;

    /// <summary>
    /// The database configuration.
    /// </summary>
    public class CoreConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Gets the hostname.
        /// </summary>
        [ConfigurationProperty("debugChannel", IsRequired = true, DefaultValue = "##helpmebot")]
        public string DebugChannel
        {
            get
            {
                return (string)base["debugChannel"];
            }
        }

        /// <summary>
        /// Gets the user agent.
        /// </summary>
        [ConfigurationProperty("useragent", IsRequired = true, DefaultValue = "Helpmebot/6.0 (+http://helpmebot.org.uk)")]
        public string UserAgent
        {
            get
            {
                return (string)base["useragent"];
            }
        }

        /// <summary>
        /// Gets the user agent.
        /// </summary>
        [ConfigurationProperty("httpTimeout", IsRequired = true, DefaultValue = 5000)]
        public int HttpTimeout
        {
            get
            {
                return (int)base["httpTimeout"];
            }
        }
    }
}
