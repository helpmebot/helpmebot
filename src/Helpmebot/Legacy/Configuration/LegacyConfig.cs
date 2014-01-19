// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyConfig.cs" company="Helpmebot Development Team">
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
    using System.Collections.Generic;

    using Castle.Core.Logging;

    using Helpmebot.Legacy.Database;

    using MySql.Data.MySqlClient;

    /// <summary>
    ///     Handles all configuration settings of the bot
    /// </summary>
    internal class LegacyConfig
    {
        #region Static Fields

        /// <summary>
        ///     The _singleton.
        /// </summary>
        private static LegacyConfig singleton;

        #endregion

        #region Fields

        /// <summary>
        ///     The _configuration cache.
        /// </summary>
        private readonly Dictionary<string, ConfigurationSetting> configurationCache;

        /// <summary>
        /// The legacy database.
        /// </summary>
        private readonly LegacyDatabase legacyDatabase = LegacyDatabase.Singleton();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initialises a new instance of the <see cref="LegacyConfig" /> class.
        /// </summary>
        protected LegacyConfig()
        {
            this.configurationCache = new Dictionary<string, ConfigurationSetting>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        #endregion

        #region Public Indexers

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified global option.
        /// </summary>
        /// <param name="globalOption">
        /// The global Option.
        /// </param>
        /// <value>
        /// </value>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string this[string globalOption]
        {
            get
            {
                return this.GetGlobalSetting(globalOption);
            }

            set
            {
                this.SetGlobalOption(globalOption, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified local option.
        /// </summary>
        /// <param name="localOption">
        /// The local Option.
        /// </param>
        /// <param name="locality">
        /// The locality.
        /// </param>
        /// <value>
        /// </value>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string this[string localOption, string locality]
        {
            get
            {
                return this.legacyDatabase.ProcHmbGetLocalOption(localOption, locality);
            }

            set
            {
                this.SetLocalOption(locality, localOption, value);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Singletons this instance.
        /// </summary>
        /// <returns>
        ///     The <see cref="LegacyConfig" />.
        /// </returns>
        public static LegacyConfig Singleton()
        {
            return singleton ?? (singleton = new LegacyConfig());
        }

        /// <summary>
        ///     The clear cache.
        /// </summary>
        public void ClearCache()
        {
            lock (this.configurationCache)
            {
                this.configurationCache.Clear();
            }
        }

        /// <summary>
        /// The get channel id.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetChannelId(string channel)
        {
            var q = new LegacyDatabase.Select("channel_id");
            q.SetFrom("channel");
            q.AddWhere(new LegacyDatabase.WhereConds("channel_name", channel));

            return this.legacyDatabase.ExecuteScalarSelect(q);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get global setting.
        /// </summary>
        /// <param name="optionName">
        /// The option name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetGlobalSetting(string optionName)
        {
            lock (this.configurationCache)
            {
                if (this.configurationCache.ContainsKey(optionName))
                {
                    ConfigurationSetting setting;
                    if (!this.configurationCache.TryGetValue(optionName, out setting))
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    if (setting.IsValid())
                    {
                        return setting.Value;
                    }

                    // option cache is not valid
                    // fetch new item from database
                    string optionValue1 = this.RetrieveOptionFromDatabase(optionName);

                    setting.Value = optionValue1;
                    this.configurationCache.Remove(optionName);
                    this.configurationCache.Add(optionName, setting);
                    return setting.Value;
                }
            }

            string optionValue2 = this.RetrieveOptionFromDatabase(optionName);

            if (optionValue2 != string.Empty)
            {
                var cachedSetting = new ConfigurationSetting(optionName, optionValue2);
                lock (this.configurationCache)
                {
                    this.configurationCache.Add(optionName, cachedSetting);
                }
            }

            return optionValue2;
        }

        /// <summary>
        /// The get option id.
        /// </summary>
        /// <param name="optionName">
        /// The option name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetOptionId(string optionName)
        {
            var q = new LegacyDatabase.Select("configuration_id");
            q.SetFrom("configuration");
            q.AddWhere(new LegacyDatabase.WhereConds("configuration_name", optionName));

            return this.legacyDatabase.ExecuteScalarSelect(q);
        }

        /// <summary>
        /// The retrieve option from database.
        /// </summary>
        /// <param name="optionName">
        /// The option name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string RetrieveOptionFromDatabase(string optionName)
        {
            try
            {
                var q = new LegacyDatabase.Select("configuration_value");
                q.SetFrom("configuration");
                q.AddLimit(1, 0);
                q.AddWhere(new LegacyDatabase.WhereConds("configuration_name", optionName));

                string result = this.legacyDatabase.ExecuteScalarSelect(q) ?? string.Empty;
                return result;
            }
            catch (Exception ex)
            {
                this.Log.Error(ex.Message, ex);
            }

            return null;
        }

        /// <summary>
        /// The set global option.
        /// </summary>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        /// <param name="optionName">
        /// The option name.
        /// </param>
        private void SetGlobalOption(string newValue, string optionName)
        {
            var vals = new Dictionary<string, string> { { "configuration_value", newValue } };
            this.legacyDatabase.Update("configuration", vals, 1, new LegacyDatabase.WhereConds("configuration_name", optionName));
        }

        /// <summary>
        /// The set local option.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="optionName">
        /// The option name.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        private void SetLocalOption(string channel, string optionName, string newValue)
        {
            string channelId = this.GetChannelId(channel);

            string configId = this.GetOptionId(optionName);

            // does setting exist in local table?
            // INNER JOIN `channel` ON `channel_id` = `cc_channel` WHERE `channel_name` = '##helpmebot' AND `configuration_name` = 'silence'
            if (newValue == null)
            {
                var deleteCommand = new MySqlCommand("DELETE FROM channelconfig WHERE cc_config = @config AND cc_channel = @channel LIMIT 1;");
                deleteCommand.Parameters.AddWithValue("@config", this.GetOptionId(optionName));
                deleteCommand.Parameters.AddWithValue("@channel", this.GetChannelId(channelId));

                this.legacyDatabase.ExecuteCommand(deleteCommand);

                return;
            }

            var q = new LegacyDatabase.Select("COUNT(*)");
            q.SetFrom("channelconfig");
            q.AddWhere(new LegacyDatabase.WhereConds("cc_channel", channelId));
            q.AddWhere(new LegacyDatabase.WhereConds("cc_config", configId));
            string count = this.legacyDatabase.ExecuteScalarSelect(q);

            if (count == "1")
            {
                // yes: Update
                var vals = new Dictionary<string, string> { { "cc_value", newValue } };
                this.legacyDatabase.Update(
                    "channelconfig", 
                    vals, 
                    1, 
                    new LegacyDatabase.WhereConds("cc_channel", channelId), 
                    new LegacyDatabase.WhereConds("cc_config", configId));
            }
            else
            {
                // no: Insert
                this.legacyDatabase.Insert("channelconfig", channelId, configId, newValue);
            }
        }

        #endregion
    }
}