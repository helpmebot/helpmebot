// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewbieWelcomer.cs" company="Helpmebot Development Team">
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
//   Newbie welcomer subsystem
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Monitoring
{
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Helpmebot;

    /// <summary>
    /// Newbie welcomer subsystem
    /// </summary>
    internal class NewbieWelcomer
    {
        private static NewbieWelcomer _instance;

        protected NewbieWelcomer()
        {
            try
            {
                _hostNames = BinaryStore.retrieve("newbie_hostnames");
            }
            catch (SerializationException ex)
            {
                GlobalFunctions.errorLog(ex);
                _hostNames = new SerializableArrayList();
            }

            try
            {
                _ignoredNicknames = BinaryStore.retrieve("newbie_ignorednicks");
            }
            catch (SerializationException ex)
            {
                GlobalFunctions.errorLog(ex);
                _ignoredNicknames = new SerializableArrayList();
            }
        }

        public static NewbieWelcomer instance()
        {
            return _instance ?? (_instance = new NewbieWelcomer());
        }

        private readonly SerializableArrayList _hostNames; 
        private readonly SerializableArrayList _ignoredNicknames;

        /// <summary>
        /// Executes the newbie.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="channel">The channel.</param>
        public void execute(User source, string channel)
        {
            Logger.instance().addToLog("Executing newbie welcomer: " + channel, Logger.LogTypes.Command);

            if (Configuration.singleton()["silence", channel] != "false" ||
                Configuration.singleton()["welcomeNewbie", channel] != "true") return;

            Logger.instance().addToLog("NW: config OK", Logger.LogTypes.Command);

            {
                var match = false;
                foreach (var pattern in _hostNames.Cast<string>())
                {
                    Logger.instance().addToLog("Checking: " + pattern + " == " + source.hostname, Logger.LogTypes.Command);
                
                    var rX = new Regex(pattern);

                    if (!rX.IsMatch(source.hostname)) continue;

                    Logger.instance().addToLog("Matched pattern", Logger.LogTypes.Command);
                    match = true;
                    break;
                }

                if (!match) return;
            }

            {
                var match = false;
                Logger.instance().addToLog("Checking ignored nicks...", Logger.LogTypes.Command);

                foreach (var pattern in _ignoredNicknames.Cast<string>())
                {
                    Logger.instance().addToLog("Checking: " + pattern + " == " + source.nickname, Logger.LogTypes.Command);
                    var rX = new Regex(pattern);

                    if (!rX.IsMatch(source.nickname)) continue;

                    Logger.instance().addToLog("Matched pattern", Logger.LogTypes.Command);
                    match = true;
                    break;
                }

                if (match) return;
            }

            string[] cmdArgs = {source.nickname, channel};
            Helpmebot6.irc.ircPrivmsg(channel, new Message().get("WelcomeMessage-" + channel.Replace("#", ""), cmdArgs));
        }

        /// <summary>
        /// Adds a host to the list of detected newbie hosts.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="except">Add to exemption list instead </param>
        public void addHost(string host, bool except = false)
        {
            if (except)
            {
                _ignoredNicknames.Add(host);
            }
            else
            {
                _hostNames.Add(host);
            }

            saveHostnames();
        }


        /// <param name="host"> The host.</param>
        /// <param name="except">Add to exemption list instead </param>
        public void delHost(string host, bool except = false)
        {
            if (except)
            {
                _ignoredNicknames.Remove(host);
            }
            else
            {
                _hostNames.Remove(host);
            }

            saveHostnames();
        }

        public string[] getHosts(bool except = false)
        {
            var data = except ? _ignoredNicknames : _hostNames;

            var list = new string[data.Count];
            data.CopyTo(list);
            return list;
        }

        private void saveHostnames()
        {
            BinaryStore.storeValue("newbie_hostnames", _hostNames);
            BinaryStore.storeValue("newbie_ignorednicks", _ignoredNicknames);
        }


    }
}