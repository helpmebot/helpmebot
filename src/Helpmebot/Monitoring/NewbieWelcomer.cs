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
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Castle.Core.Logging;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Newbie welcomer subsystem
    /// </summary>
    internal class NewbieWelcomer
    {
        /// <summary>
        /// The singletonInstance.
        /// </summary>
        private static NewbieWelcomer singletonInstance;

        /// <summary>
        /// The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// The host names.
        /// </summary>
        private readonly SerializableArrayList hostNames;

        /// <summary>
        /// The ignored nicknames.
        /// </summary>
        private readonly SerializableArrayList ignoredNicknames;

        /// <summary>
        /// Initialises a new instance of the <see cref="NewbieWelcomer"/> class.
        /// </summary>
        protected NewbieWelcomer()
        {
            // FIXME: Remove me!
            this.Log = ServiceLocator.Current.GetInstance<ILogger>();
            this.messageService = ServiceLocator.Current.GetInstance<IMessageService>();

            try
            {
                this.hostNames = BinaryStore.retrieve("newbie_hostnames");
            }
            catch (SerializationException ex)
            {
                this.Log.Error(ex.Message, ex);
                this.hostNames = new SerializableArrayList();
            }

            try
            {
                this.ignoredNicknames = BinaryStore.retrieve("newbie_ignorednicks");
            }
            catch (SerializationException ex)
            {
                this.Log.Error(ex.Message, ex);
                this.ignoredNicknames = new SerializableArrayList();
            }
        }

        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        /// The instance.
        /// </summary>
        /// <returns>
        /// The <see cref="NewbieWelcomer"/>.
        /// </returns>
        public static NewbieWelcomer Instance()
        {
            return singletonInstance ?? (singletonInstance = new NewbieWelcomer());
        }

        /// <summary>
        /// Executes the newbie.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="channel">The channel.</param>
        public void Execute(User source, string channel)
        {
            this.Log.Debug(string.Format("Executing newbie welcomer: {0}", channel));

            if (LegacyConfig.singleton()["silence", channel] != "false"
                || LegacyConfig.singleton()["welcomeNewbie", channel] != "true")
            {
                return;
            }

            this.Log.Debug("NewbieWelcomer - config OK");
            
            {
                var match = false;
                foreach (var pattern in this.hostNames.Cast<string>())
                {
                    this.Log.Debug(string.Format("Checking {0} == {1}", pattern, source.hostname));

                    var rX = new Regex(pattern);

                    if (!rX.IsMatch(source.hostname))
                    {
                        continue;
                    }

                    this.Log.Debug("Matched pattern");
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
                this.Log.Debug("Checking ignored nicks...");

                foreach (var pattern in this.ignoredNicknames.Cast<string>())
                {
                    this.Log.Debug(string.Format("Checking {0} == {1}", pattern, source.hostname));
                    
                    var rX = new Regex(pattern);

                    if (!rX.IsMatch(source.nickname))
                    {
                        continue;
                    }

                    this.Log.Debug("Matched pattern");
                    match = true;
                    break;
                }

                if (match)
                {
                    return;
                }
            }

            string[] cmdArgs = { source.nickname, channel };
            string message = this.messageService.RetrieveMessage(
                "WelcomeMessage-" + channel.Replace("#", string.Empty),
                channel,
                cmdArgs);

            Helpmebot6.irc.IrcPrivmsg(channel, message);
        }

        /// <summary>
        /// Adds a host to the list of detected newbie hosts.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="except">Add to exemption list instead </param>
        public void AddHost(string host, bool except = false)
        {
            if (except)
            {
                this.ignoredNicknames.Add(host);
            }
            else
            {
                this.hostNames.Add(host);
            }

            this.SaveHostnames();
        }

        /// <summary>
        /// The delete host.
        /// </summary>
        /// <param name="host">
        /// The host.
        /// </param>
        /// <param name="except">
        /// Add to exemption list instead 
        /// </param>
        public void DeleteHost(string host, bool except = false)
        {
            if (except)
            {
                this.ignoredNicknames.Remove(host);
            }
            else
            {
                this.hostNames.Remove(host);
            }

            this.SaveHostnames();
        }

        /// <summary>
        /// The get hosts.
        /// </summary>
        /// <param name="except">
        /// The except.
        /// </param>
        /// <returns>
        /// The list of hosts.
        /// </returns>
        public string[] GetHosts(bool except = false)
        {
            var data = except ? this.ignoredNicknames : this.hostNames;

            var list = new string[data.Count];
            data.CopyTo(list);
            return list;
        }

        /// <summary>
        /// The save hostnames.
        /// </summary>
        private void SaveHostnames()
        {
            BinaryStore.storeValue("newbie_hostnames", this.hostNames);
            BinaryStore.storeValue("newbie_ignorednicks", this.ignoredNicknames);
        }
    }
}