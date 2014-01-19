// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationSetting.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Legacy.Configuration
{
    using System;

    using Castle.Core.Logging;

    /// <summary>
    ///     Represents a configuration setting
    /// </summary>
    internal class ConfigurationSetting
    {
        #region Constants

        /// <summary>
        /// The cache timeout.
        /// </summary>
        private const double CacheTimeout = 5;

        #endregion

        #region Fields

        /// <summary>
        /// The _setting name.
        /// </summary>
        private readonly string settingName;

        /// <summary>
        /// The _last retrieval.
        /// </summary>
        private DateTime lastRetrieval;

        /// <summary>
        /// The _setting value.
        /// </summary>
        private string settingValue;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="ConfigurationSetting"/> class. 
        /// Initializes a new instance of the <see cref="ConfigurationSetting"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public ConfigurationSetting(string name, string value)
        {
            this.settingName = name;
            this.settingValue = value;
            this.lastRetrieval = DateTime.Now;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        ///     Gets the name of this setting.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return this.settingName;
            }
        }

        /// <summary>
        ///     Gets or sets the value of this setting.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get
            {
                return this.settingValue;
            }

            set
            {
                this.settingValue = value;
                this.lastRetrieval = DateTime.Now;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.settingName;
        }

        /// <summary>
        ///     Determines whether this instance is valid.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            try
            {
                TimeSpan difference = DateTime.Now - this.lastRetrieval;
                return difference.TotalMinutes <= CacheTimeout;
            }
            catch (Exception ex)
            {
                this.Log.Error(ex.Message, ex);
            }

            return false;
        }

        #endregion
    }
}