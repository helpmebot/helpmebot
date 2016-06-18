// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivateConfiguration.cs" company="Helpmebot Development Team">
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

    using MySql.Data.MySqlClient;

    /// <summary>
    ///     The database configuration.
    /// </summary>
    public class PrivateConfiguration : ConfigurationSection, IPrivateConfiguration
    {
        #region Fields

        /// <summary>
        ///     The connection string.
        /// </summary>
        private MySqlConnectionStringBuilder connectionString;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the connection string.
        /// </summary>
        public MySqlConnectionStringBuilder ConnectionString
        {
            get
            {
                if (this.connectionString == null)
                {
                    this.connectionString = new MySqlConnectionStringBuilder
                                                {
                                                    Database = this.Schema, 
                                                    Password = this.Password, 
                                                    Server = this.Hostname, 
                                                    UserID = this.Username, 
                                                    Port = (uint)this.Port
                                                };
                }

                return this.connectionString;
            }
        }

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
        ///     Gets the password.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get
            {
                return (string)base["password"];
            }
        }

        /// <summary>
        ///     Gets the port.
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = 3306)]
        public int Port
        {
            get
            {
                return (int)base["port"];
            }
        }

        /// <summary>
        ///     Gets the schema.
        /// </summary>
        [ConfigurationProperty("schema", IsRequired = true)]
        public string Schema
        {
            get
            {
                return (string)base["schema"];
            }
        }

        /// <summary>
        ///     Gets the username.
        /// </summary>
        [ConfigurationProperty("username", IsRequired = true)]
        public string Username
        {
            get
            {
                return (string)base["username"];
            }
        }

        /// <summary>
        ///     Gets the IRC password.
        /// </summary>
        [ConfigurationProperty("ircPassword", IsRequired = true)]
        public string IrcPassword
        {
            get
            {
                return (string)base["ircPassword"];
            }
        }

        /// <summary>
        ///     Gets the IpInfoDB API key.
        /// </summary>
        [ConfigurationProperty("ipInfoDbApiKey", DefaultValue = "")]
        public string IpInfoDbApiKey
        {
            get
            {
                return (string)base["ipInfoDbApiKey"];
            }
        }

        /// <summary>
        ///     Gets the Google API key.
        /// </summary>
        [ConfigurationProperty("googleApiKey", DefaultValue = "")]
        public string GoogleApiKey
        {
            get
            {
                return (string)base["googleApiKey"];
            }
        }

        /// <summary>
        ///     Gets the MaxMind Database Path.
        /// </summary>
        [ConfigurationProperty("maxMindDatabasePath", DefaultValue = "")]
        public string MaxMindDatabasePath
        {
            get
            {
                return (string)base["maxMindDatabasePath"];
            }
        }

        #endregion
    }
}