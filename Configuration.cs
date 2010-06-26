/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

#endregion

namespace helpmebot6
{
    public class Configuration
    {
        private readonly DAL _dbal = DAL.singleton();

        private static Configuration _singleton;

        public static Configuration singleton()
        {
            return _singleton ?? ( _singleton = new Configuration( ) );
        }

        protected Configuration()
        {
            this._configurationCache = new ArrayList();
        }


        private readonly ArrayList _configurationCache;

        public string this[string globalOption]
        {
            get { return retrieveGlobalStringOption(globalOption); }
            set { setGlobalOption(globalOption, value); }
        }

        public string retrieveGlobalStringOption(string optionName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);


            foreach (ConfigurationSetting s in this._configurationCache)
            {
                if ( s.name != optionName ) continue;

                // option found, deal with option

                if (s.isValid())
                {
                    //option cache is still valid
                    return s.value;
                }
                //option cache is not valid
                // fetch new item from database
                string optionValue1 = this.retrieveOptionFromDatabase(optionName);

                s.value = optionValue1;
                return s.value;
            }

            // option not found, add entry to cache
            string optionValue2 = retrieveOptionFromDatabase(optionName);

            if (optionValue2 != string.Empty)
            {
                ConfigurationSetting cachedSetting = new ConfigurationSetting(optionName, optionValue2);
                this._configurationCache.Add(cachedSetting);
            }
            return optionValue2;
        }

        public uint retrieveGlobalUintOption(string optionName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string optionValue = retrieveGlobalStringOption(optionName);
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

        public string retrieveLocalStringOption(string optionName, string channel)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return this._dbal.proc_HMB_GET_LOCAL_OPTION(optionName, channel);
        }

        private string retrieveOptionFromDatabase(string optionName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

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

        public void setGlobalOption(string optionName, string newValue)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            Dictionary<string, string> vals = new Dictionary<string, string>
                                                  {
                                                      {
                                                          "configuration_value",
                                                          newValue
                                                          }
                                                  };
            this._dbal.update("configuration", vals, 1, new DAL.WhereConds("configuration_name", optionName));
        }

        public void setLocalOption(string optionName, string channel, string newValue)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            // convert channel to ID


            string channelId = getChannelId(channel);

            string configId = getOptionId(optionName);

            // does setting exist in local table?
            //  INNER JOIN `channel` ON `channel_id` = `cc_channel` WHERE `channel_name` = '##helpmebot' AND `configuration_name` = 'silence'

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

        public void setOption(string optionName, string target, string newValue)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (target == "global")
            {
                setGlobalOption(optionName, newValue);
            }
            else
            {
                setLocalOption(optionName, target, newValue);
            }
        }

        public void deleteLocalOption(string optionName, string target)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            this._dbal.delete("channelconfig", 1, new DAL.WhereConds("cc_config", getOptionId(optionName)),
                        new DAL.WhereConds("cc_channel", getChannelId(target)));
        }

        private string getOptionId(string optionName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            DAL.Select q = new DAL.Select("configuration_id");
            q.setFrom("configuration");
            q.addWhere(new DAL.WhereConds("configuration_name", optionName));

            return this._dbal.executeScalarSelect(q);
        }

        public string getChannelId(string channel)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            DAL.Select q = new DAL.Select("channel_id");
            q.setFrom("channel");
            q.addWhere(new DAL.WhereConds("channel_name", channel));

            return this._dbal.executeScalarSelect(q);
        }

        public static void readHmbotConfigFile(string filename,
                                               ref string mySqlServerHostname, ref string mySqlUsername,
                                               ref string mySqlPassword, ref uint mySqlServerPort,
                                               ref string mySqlSchema)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            StreamReader settingsreader = new StreamReader(filename);
            mySqlServerHostname = settingsreader.ReadLine();
            mySqlServerPort = uint.Parse(settingsreader.ReadLine());
            mySqlUsername = settingsreader.ReadLine();
            mySqlPassword = settingsreader.ReadLine();
            mySqlSchema = settingsreader.ReadLine();
            settingsreader.Close();
        }

        #region messaging

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

        private static string parseMessage(string messageFormat, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return String.Format(messageFormat, args);
        }

        public string getMessage(string messageName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return chooseRandomMessage(messageName);
        }

        public string getMessage(string messageName, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return parseMessage(chooseRandomMessage(messageName), args);
        }

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