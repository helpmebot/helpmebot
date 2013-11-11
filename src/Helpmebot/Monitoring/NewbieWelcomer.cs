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

namespace Helpmebot.Monitoring
{
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;

    using log4net;

    /// <summary>
    /// Newbie welcomer subsystem
    /// </summary>
    internal class NewbieWelcomer
    {
        private static readonly ILog log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static NewbieWelcomer _instance;

        protected NewbieWelcomer()
        {
            try
            {
                this._hostNames = BinaryStore.retrieve("newbie_hostnames");
            }
            catch (SerializationException ex)
            {
                log.Error(ex.Message, ex);
                this._hostNames = new SerializableArrayList();
            }

            try
            {
                this._ignoredNicknames = BinaryStore.retrieve("newbie_ignorednicks");
            }
            catch (SerializationException ex)
            {
                log.Error(ex.Message, ex);
                this._ignoredNicknames = new SerializableArrayList();
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
            log.Debug(string.Format("Executing newbie welcomer: {0}", channel));

            if (LegacyConfig.singleton()["silence", channel] != "false"
                || LegacyConfig.singleton()["welcomeNewbie", channel] != "true")
            {
                return;
            }

            log.Debug("NewbieWelcomer - config OK");
            
            {
                var match = false;
                foreach (var pattern in this._hostNames.Cast<string>())
                {
                    log.Debug(string.Format("Checking {0} == {1}", pattern, source.hostname));

                    var rX = new Regex(pattern);

                    if (!rX.IsMatch(source.hostname))
                    {
                        continue;
                    }

                    log.Debug("Matched pattern");
                    match = true;
                    break;
                }

                if (!match)
                {
                    return;
                }
            }

            {
                var match = false;
                log.Debug("Checking ignored nicks...");

                foreach (var pattern in this._ignoredNicknames.Cast<string>())
                {
                    log.Debug(string.Format("Checking {0} == {1}", pattern, source.hostname));
                    
                    var rX = new Regex(pattern);

                    if (!rX.IsMatch(source.nickname))
                    {
                        continue;
                    }

                    log.Debug("Matched pattern");
                    match = true;
                    break;
                }

                if (match)
                {
                    return;
                }
            }

            string[] cmdArgs = {source.nickname, channel};
            Helpmebot6.irc.IrcPrivmsg(channel, new Message().GetMessage("WelcomeMessage-" + channel.Replace("#", string.Empty), cmdArgs));
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
                this._ignoredNicknames.Add(host);
            }
            else
            {
                this._hostNames.Add(host);
            }

            this.saveHostnames();
        }


        /// <param name="host"> The host.</param>
        /// <param name="except">Add to exemption list instead </param>
        public void delHost(string host, bool except = false)
        {
            if (except)
            {
                this._ignoredNicknames.Remove(host);
            }
            else
            {
                this._hostNames.Remove(host);
            }

            this.saveHostnames();
        }

        public string[] getHosts(bool except = false)
        {
            var data = except ? this._ignoredNicknames : this._hostNames;

            var list = new string[data.Count];
            data.CopyTo(list);
            return list;
        }

        private void saveHostnames()
        {
            BinaryStore.storeValue("newbie_hostnames", this._hostNames);
            BinaryStore.storeValue("newbie_ignorednicks", this._ignoredNicknames);
        }


    }
}