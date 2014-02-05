// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CoreConfiguration.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Configuration.XmlSections
{
    using System.Configuration;

    using Helpmebot.Configuration.XmlSections.Interfaces;

    /// <summary>
    ///     The database configuration.
    /// </summary>
    public class CoreConfiguration : ConfigurationSection, ICoreConfiguration
    {
        #region Public Properties

        /// <summary>
        ///     Gets the debug channel.
        /// </summary>
        [ConfigurationProperty("debugChannel", DefaultValue = "##helpmebot")]
        public string DebugChannel
        {
            get
            {
                return (string)base["debugChannel"];
            }
        }

        /// <summary>
        ///     Gets the HTTP timeout
        /// </summary>
        [ConfigurationProperty("httpTimeout", DefaultValue = 5000)]
        public int HttpTimeout
        {
            get
            {
                return (int)base["httpTimeout"];
            }
        }

        /// <summary>
        ///     Gets the user agent.
        /// </summary>
        [ConfigurationProperty(
            "useragent", 
            DefaultValue = "Helpmebot/6.0 (+http://helpmebot.org.uk)")]
        public string UserAgent
        {
            get
            {
                return (string)base["useragent"];
            }
        }

        /// <summary>
        ///     Gets the monitoring port
        /// </summary>
        [ConfigurationProperty("monitorPort", DefaultValue = 62167)]
        public int MonitorPort
        {
            get
            {
                return (int)base["monitorPort"];
            }
        }        
        
        /// <summary>
        ///     Gets the default command trigger
        /// </summary>
        [ConfigurationProperty("commandTrigger", DefaultValue = "!")]
        public string CommandTrigger
        {
            get
            {
                return (string)base["commandTrigger"];
            }
        }

        #endregion
    }
}