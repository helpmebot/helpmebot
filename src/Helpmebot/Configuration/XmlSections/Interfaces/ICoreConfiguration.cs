// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICoreConfiguration.cs" company="Helpmebot Development Team">
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

namespace Helpmebot.Configuration.XmlSections.Interfaces
{
    using System.Configuration;

    /// <summary>
    /// The CoreConfiguration interface.
    /// </summary>
    public interface ICoreConfiguration
    {
        #region Public Properties

        /// <summary>
        ///     Gets the debug channel.
        /// </summary>
        [ConfigurationProperty("debugChannel", IsRequired = true, DefaultValue = "##helpmebot")]
        string DebugChannel { get; }

        /// <summary>
        ///     Gets the http timeout.
        /// </summary>
        [ConfigurationProperty("httpTimeout", IsRequired = true, DefaultValue = 5000)]
        int HttpTimeout { get; }

        /// <summary>
        ///     Gets the user agent.
        /// </summary>
        [ConfigurationProperty("useragent", IsRequired = true, DefaultValue = "Helpmebot/6.4 (+https://helpmebot.org.uk)")]
        string UserAgent { get; }

        /// <summary>
        ///     Gets the default command trigger
        /// </summary>
        [ConfigurationProperty("commandTrigger", DefaultValue = "!")]
        string CommandTrigger { get; }

        #endregion
    }
}