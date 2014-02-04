// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIrcConfiguration.cs" company="Helpmebot Development Team">
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
    ///     The IRC Configuration interface.
    /// </summary>
    public interface IIrcConfiguration
    {
        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether to authenticate to services.
        /// </summary>
        [ConfigurationProperty("authToServices", IsRequired = true, DefaultValue = true)]
        bool AuthToServices { get; }

        /// <summary>
        ///     Gets the hostname.
        /// </summary>
        [ConfigurationProperty("hostname", IsRequired = true)]
        string Hostname { get; }

        /// <summary>
        ///     Gets the nickname.
        /// </summary>
        [ConfigurationProperty("nickname", IsRequired = true)]
        string Nickname { get; }

        /// <summary>
        ///     Gets the hostname.
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        int Port { get; }

        /// <summary>
        /// Gets the real name.
        /// </summary>
        [ConfigurationProperty("realname", IsRequired = true)]
        string RealName { get; }

        /// <summary>
        /// Gets the username.
        /// </summary>
        [ConfigurationProperty("username", IsRequired = true)]
        string Username { get; }

        #endregion
    }
}