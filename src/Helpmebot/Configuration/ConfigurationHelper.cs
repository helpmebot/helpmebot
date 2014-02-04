// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationHelper.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Configuration
{
    using System.Configuration;

    using Helpmebot.Configuration.XmlSections.Interfaces;

    /// <summary>
    ///     The configuration helper.
    /// </summary>
    public class ConfigurationHelper : IConfigurationHelper
    {
        #region Fields

        /// <summary>
        ///     The core configuration.
        /// </summary>
        private ICoreConfiguration coreConfiguration;

        /// <summary>
        ///     The private configuration.
        /// </summary>
        private IPrivateConfiguration privateConfiguration;

        /// <summary>
        /// The IRC configuration.
        /// </summary>
        private IIrcConfiguration ircConfiguration;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the core configuration.
        /// </summary>
        public ICoreConfiguration CoreConfiguration
        {
            get
            {
                return this.coreConfiguration
                       ?? (this.coreConfiguration = ConfigurationManager.GetSection("core") as ICoreConfiguration);
            }
        }

        /// <summary>
        ///     Gets the database configuration.
        /// </summary>
        /// <returns>
        ///     The <see cref="PrivateConfiguration" />.
        /// </returns>
        public IPrivateConfiguration PrivateConfiguration
        {
            get
            {
                return this.privateConfiguration
                       ?? (this.privateConfiguration =
                           ConfigurationManager.GetSection("private") as IPrivateConfiguration);
            }
        }

        /// <summary>
        /// Gets the IRC configuration.
        /// </summary>
        public IIrcConfiguration IrcConfiguration
        {
            get
            {
                return this.ircConfiguration
                       ?? (this.ircConfiguration = ConfigurationManager.GetSection("irc") as IIrcConfiguration);
            }
        }

        #endregion
    }
}