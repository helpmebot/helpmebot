// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatabaseConfiguration.cs" company="Helpmebot Development Team">
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
// <summary>
//   Defines the DatabaseConfiguration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Configuration.XmlSections
{
    using System.Configuration;

    /// <summary>
    /// The database configuration.
    /// </summary>
    public class DatabaseConfiguration : ConfigurationSection
    {
        public DatabaseConfiguration()
        {
        }

        /// <summary>
        /// Gets the hostname.
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
        /// Gets the port.
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = 3306, IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)base["port"];
            }
        }

        /// <summary>
        /// Gets the schema.
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
        /// Gets the username.
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
        /// Gets the password.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get
            {
                return (string)base["password"];
            }
        }
    }
}
