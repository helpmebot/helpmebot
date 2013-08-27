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
// <summary>
//   Represents a configuration setting
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6
{
    using System;

    /// <summary>
    /// Represents a configuration setting
    /// </summary>
    internal class ConfigurationSetting
    {
        private const double CACHE_TIMEOUT = 5;

        private string _settingValue;
        private readonly string _settingName;
        private DateTime _lastRetrieval;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSetting"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public ConfigurationSetting(string name, string value)
        {
            _settingName = name;
            _settingValue = value;
            _lastRetrieval = DateTime.Now;
        }

        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool isValid()
        {
            try
            {
                TimeSpan difference = DateTime.Now - _lastRetrieval;
                return difference.TotalMinutes <= CACHE_TIMEOUT;
            }
            catch (Exception ex)
            {
                GlobalFunctions.errorLog(ex);
            }
            return false;
        }

        /// <summary>
        /// Gets or sets the value of this setting.
        /// </summary>
        /// <value>The value.</value>
        public string value
        {
            get { return _settingValue; }
            set
            {
                _settingValue = value;
                _lastRetrieval = DateTime.Now;
            }
        }

        /// <summary>
        /// Gets the name of this setting.
        /// </summary>
        /// <value>The name.</value>
        public string name
        {
            get { return _settingName; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _settingName;
        }
    }
}