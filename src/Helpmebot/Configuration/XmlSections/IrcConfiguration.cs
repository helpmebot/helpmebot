// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcConfiguration.cs" company="Helpmebot Development Team">
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
    public class IrcConfiguration : ConfigurationSection, IIrcConfiguration
    {
        #region Public Properties

        /// <summary>
        ///     Gets the hostname.
        /// </summary>
        [ConfigurationProperty("hostname", IsRequired = true)]
        public string Hostname
        {
            get
            {
                return (string)base["hostname"];
            }
        }

        /// <summary>
        ///     Gets the hostname.
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)base["port"];
            }
        }

        /// <summary>
        ///     Gets a value indicating whether to authenticate to services
        /// </summary>
        [ConfigurationProperty(
            "authToServices",
            IsRequired = true,
            DefaultValue = true)]
        public bool AuthToServices
        {
            get
            {
                return (bool)base["authToServices"];
            }
        }

        #endregion
    }
}