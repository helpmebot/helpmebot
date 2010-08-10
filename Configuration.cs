// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

#endregion

namespace helpmebot6
{
    /// <summary>
    /// Handles all configuration settings of the bot
    /// </summary>
    internal class Configuration
    {

        #region config

        private readonly DAL _dbal = DAL.singleton();

        private static Configuration _singleton;

        /// <summary>
        /// Singletons this instance.
        /// </summary>
        /// <returns></returns>
        public static Configuration singleton()
        {
            return _singleton ?? ( _singleton = new Configuration( ) );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        protected Configuration()
        {
            this._configurationCache = new Dictionary<string, ConfigurationSetting>();
        }

        private readonly Dictionary<string, ConfigurationSetting> _configurationCache;

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified global option.
        /// </summary>
        /// <value></value>
        public string this[string globalOption]
        {
            get { return this.getGlobalSetting(globalOption); }
            set { this.setGlobalOption(globalOption, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified local option.
        /// </summary>
        /// <value></value>
        public string this[string localOption, string locality]
        {
            get
            {
                return this._dbal.proc_HMB_GET_LOCAL_OPTION(localOption, locality);
            }
            set
            {
                this.setLocalOption( locality, localOption, value );
            }
        }

        [Obsolete("Use indexer property instead.")]
        public string retrieveGlobalStringOption(string optionName)
        {
            return getGlobalSetting( optionName );
        }

        private string getGlobalSetting( string optionName )
        {
            if( this._configurationCache.ContainsKey( optionName ))
            {
                ConfigurationSetting setting;
                if(this._configurationCache.TryGetValue(optionName,out setting  ))
                {
                    if ( setting.isValid( ) )
                    {
                        return setting.value;
                    }

                    //option cache is not valid
                    // fetch new item from database
                    string optionValue1 = this.retrieveOptionFromDatabase( optionName );

                    setting.value = optionValue1;
                    this._configurationCache.Remove( optionName );
                    this._configurationCache.Add( optionName, setting );
                    return setting.value;
                }
                throw new ArgumentOutOfRangeException();
            }

            string optionValue2 = this.retrieveOptionFromDatabase(optionName);

            if (optionValue2 != string.Empty)
            {
                ConfigurationSetting cachedSetting = new ConfigurationSetting(optionName, optionValue2);
                this._configurationCache.Add( optionName, cachedSetting );
            }
            return optionValue2;
        }

        [Obsolete("Use indexer property instead, then cast to string.")]
        public uint retrieveGlobalUintOption(string optionName)
        {

            string optionValue = this.getGlobalSetting(optionName);
            uint value;
            try
            {
                value = uint.Parse(optionValue);
            }
            catch (Exception)
            {
                return 0;
            }
            return value;
        }

        [Obsolete("Use indexer property instead.")]
        public string retrieveLocalStringOption(string optionName, string channel)
        {
            return this._dbal.proc_HMB_GET_LOCAL_OPTION(optionName, channel);
        }

        private string retrieveOptionFromDatabase(string optionName)
        {
            try
            {
                DAL.Select q = new DAL.Select("configuration_value");
                q.setFrom("configuration");
                q.addLimit(1, 0);
                q.addWhere(new DAL.WhereConds("configuration_name", optionName));

                string result = this._dbal.executeScalarSelect(q) ?? "";
                return result;
            }
            catch (Exception ex)
            {
                GlobalFunctions.errorLog(ex);
            }
            return null;
        }

        [Obsolete("Use indexer property instead.")]
        public void oldSetGlobalOption(string optionName, string newValue)
        {
            setGlobalOption( newValue, optionName );
        }

        private void setGlobalOption( string newValue, string optionName )
        {
            Dictionary<string, string> vals = new Dictionary<string, string>
                                                  {
                                                      {
                                                          "configuration_value",
                                                          newValue
                                                          }
                                                  };
            this._dbal.update("configuration", vals, 1, new DAL.WhereConds("configuration_name", optionName));
        }

        [Obsolete("Use indexer property instead.")]
        public void oldSetLocalOption(string optionName, string channel, string newValue)
        {
            this.setLocalOption( channel, optionName, newValue );
        }

        private void setLocalOption( string channel, string optionName, string newValue )
        {
            string channelId = this.getChannelId(channel);

            string configId = this.getOptionId(optionName);

            // does setting exist in local table?
            //  INNER JOIN `channel` ON `channel_id` = `cc_channel` WHERE `channel_name` = '##helpmebot' AND `configuration_name` = 'silence'


            if(newValue == null)
            {
                this._dbal.delete( "channelconfig", 1, new DAL.WhereConds( "cc_config", getOptionId( optionName ) ),
                                   new DAL.WhereConds( "cc_channel", getChannelId( channelId ) ) );
                return;
            }

            DAL.Select q = new DAL.Select("COUNT(*)");
            q.setFrom("channelconfig");
            q.addWhere(new DAL.WhereConds("cc_channel", channelId));
            q.addWhere(new DAL.WhereConds("cc_config", configId));
            string count = this._dbal.executeScalarSelect(q);

            if (count == "1")
            {
                //yes: update
                Dictionary<string, string> vals = new Dictionary<string, string>
                                                      {
                                                          { "cc_value", newValue }
                                                      };
                this._dbal.update("channelconfig", vals, 1, new DAL.WhereConds("cc_channel", channelId),
                                  new DAL.WhereConds("cc_config", configId));
            }
            else
            {
                // no: insert
                this._dbal.insert("channelconfig", channelId, configId, newValue);
            }
        }
        
        [Obsolete("Use indexer propery instead")]
        public void setOption(string optionName, string target, string newValue)
        {

            if (target == "global")
            {
                this.oldSetGlobalOption(optionName, newValue);
            }
            else
            {
                this.setLocalOption(optionName, target, newValue);
            }
        }

        [Obsolete("Use indexer property instead with null value.")]
        public void deleteLocalOption(string optionName, string target)
        {
            this.setLocalOption( target, optionName, null );
        }

        private string getOptionId(string optionName)
        {
            DAL.Select q = new DAL.Select("configuration_id");
            q.setFrom("configuration");
            q.addWhere(new DAL.WhereConds("configuration_name", optionName));

            return this._dbal.executeScalarSelect(q);
        }

        public string getChannelId(string channel)
        {

            DAL.Select q = new DAL.Select("channel_id");
            q.setFrom("channel");
            q.addWhere(new DAL.WhereConds("channel_name", channel));

            return this._dbal.executeScalarSelect(q);
        }

        /// <summary>
        /// Reads the hmbot config file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="mySqlServerHostname">MySQL server hostname.</param>
        /// <param name="mySqlUsername">MySQL username.</param>
        /// <param name="mySqlPassword">MySQL password.</param>
        /// <param name="mySqlServerPort">MySQL server port.</param>
        /// <param name="mySqlSchema">My SQL schema.</param>
        public static void readHmbotConfigFile(string filename,
                                               ref string mySqlServerHostname, ref string mySqlUsername,
                                               ref string mySqlPassword, ref uint mySqlServerPort,
                                               ref string mySqlSchema)
        {

            StreamReader settingsreader = new StreamReader(filename);
            mySqlServerHostname = settingsreader.ReadLine();
            mySqlServerPort = uint.Parse(settingsreader.ReadLine());
            mySqlUsername = settingsreader.ReadLine();
            mySqlPassword = settingsreader.ReadLine();
            mySqlSchema = settingsreader.ReadLine();
            settingsreader.Close();
        }
#endregion
        #region messaging

        [Obsolete]
        private ArrayList getMessages(string messageName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            //"SELECT m.`message_text` FROM message m WHERE m.`message_name` = '"+messageName+"';" );

            DAL.Select q = new DAL.Select("message_text");
            q.setFrom("message");
            q.addWhere(new DAL.WhereConds("message_name", messageName));

            ArrayList resultset = this._dbal.executeSelect(q);

            ArrayList al = new ArrayList();

            foreach (object[] item in resultset)
            {
                al.Add((item)[0]);
            }
            return al;
        }

        //returns a random message chosen from the list of possible message names
        [Obsolete]
        private string chooseRandomMessage(string messageName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            Random rnd = new Random();
            ArrayList al = getMessages(messageName);
            if (al.Count == 0)
            {
                Helpmebot6.irc.ircPrivmsg(Helpmebot6.debugChannel,
                                          "***ERROR*** Message '" + messageName + "' not found in message table");
                return "";
            }
            return al[rnd.Next(0, al.Count)].ToString();
        }

        [Obsolete]
        private static string buildMessage(string messageFormat, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return String.Format(messageFormat, args);
        }

        [Obsolete]
        public string getMessage(string messageName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return chooseRandomMessage(messageName);
        }

        [Obsolete]
        public string getMessage(string messageName, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return buildMessage(chooseRandomMessage(messageName), args);
        }

        [Obsolete]
        public string getMessage(string messageName, string defaultMessageName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string msg = this.getMessage(messageName);
            if (msg == string.Empty)
            {
                msg = this.getMessage(defaultMessageName);
                this.saveMessage(messageName, "", msg);
            }
            msg = this.getMessage(messageName);
            return msg;
        }

        [Obsolete]
        public string getMessage(string messageName, string defaultMessageName, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string msg = this.getMessage(messageName, args);
            if (msg == string.Empty)
            {
                msg = this.getMessage(defaultMessageName);
                this.saveMessage(messageName, "", msg);
            }
            msg = this.getMessage(messageName, args);
            return msg;
        }

        [Obsolete]
        public void saveMessage(string messageName, string messageDescription, string messageContent)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            this._dbal.insert("message", "", messageName, messageDescription, messageContent, "1");
        }

        #endregion
    }
}