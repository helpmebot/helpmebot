// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPrivateConfiguration.cs" company="Helpmebot Development Team">
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

    using MySql.Data.MySqlClient;

    /// <summary>
    ///     The DatabaseConfiguration interface.
    /// </summary>
    public interface IPrivateConfiguration
    {
        #region Public Properties

        /// <summary>
        ///     Gets the connection string.
        /// </summary>
        MySqlConnectionStringBuilder ConnectionString { get; }

        /// <summary>
        ///     Gets the hostname.
        /// </summary>
        [ConfigurationProperty("hostname", IsRequired = true)]
        string Hostname { get; }

        /// <summary>
        ///     Gets the password.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        string Password { get; }

        /// <summary>
        ///     Gets the port.
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = 3306, IsRequired = true)]
        int Port { get; }

        /// <summary>
        ///     Gets the schema.
        /// </summary>
        [ConfigurationProperty("schema", IsRequired = true)]
        string Schema { get; }

        /// <summary>
        ///     Gets the username.
        /// </summary>
        [ConfigurationProperty("username", IsRequired = true)]
        string Username { get; }

        /// <summary>
        ///     Gets the IRC password.
        /// </summary>
        [ConfigurationProperty("ircPassword", IsRequired = true)]
        string IrcPassword { get; }

        /// <summary>
        ///     Gets the IpInfoDB API key.
        /// </summary>
        [ConfigurationProperty("ipInfoDbApiKey", DefaultValue = "")]
        string IpInfoDbApiKey { get; }

        /// <summary>
        ///     Gets the Google API key.
        /// </summary>
        [ConfigurationProperty("googleApiKey", DefaultValue = "")]
        string GoogleApiKey { get; }

        /// <summary>
        /// Gets the max mind database path.
        /// </summary>
        [ConfigurationProperty("maxMindDatabasePath", DefaultValue = "")]
        string MaxMindDatabasePath { get; }

        #endregion
    }
}