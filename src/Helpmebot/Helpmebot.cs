// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helpmebot.cs" company="Helpmebot Development Team">
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
//   Helpmebot main class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Castle.Core.Logging;
    using Castle.Windsor;
    using helpmebot6.Commands;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Startup;
    using Helpmebot.Startup.Installers;
    using Helpmebot.Threading;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;

    /// <summary>
    /// Helpmebot main class
    /// </summary>
    public class Helpmebot6
    {
        /// <summary>
        /// The start-up time.
        /// </summary>
        public static readonly DateTime StartupTime = DateTime.Now;
        
        /// <summary>
        /// The new IRC client.
        /// </summary>
        private static IIrcClient newIrc;

        /// <summary>
        /// The container.
        /// </summary>
        private static IWindsorContainer container;

        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public static ILogger Log { get; set; }

        /// <summary>
        /// The stop.
        /// </summary>
        public static void Stop()
        {
            try
            {
                ThreadList.GetInstance().Stop();
                container.Dispose();
            }
            finally 
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// The main.
        /// </summary>
        private static void Main(string[] args)
        {
            string configurationFile = "configuration.xml";
            
            if (args.Length >= 1)
            {
                configurationFile = args[0];
            }

            if (!File.Exists(configurationFile))
            {
                var fullPath = Path.GetFullPath(configurationFile);

                Console.WriteLine("Configuration file at {0} does not exist!", fullPath);
                return;
            }
            
            // DO NOT DO THIS.
            // EVER.
            // BLAME GLOBALSIGN FOR THIS.
            //
            // (please don't think any less of me for this...)
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;

            container = new WindsorContainer(configurationFile);
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
            container.Install(new MainInstaller());

            
            Log = ServiceLocator.Current.GetInstance<ILogger>();
            Log.Info("Initialising Helpmebot...");
            
            newIrc = container.Resolve<IIrcClient>();

            JoinChannels();
            
            newIrc.JoinReceivedEvent += NotifyOnJoinEvent;
            newIrc.ReceivedMessage += ReceivedMessage;
            newIrc.InviteReceivedEvent += IrcInviteEvent;
            newIrc.WasKickedEvent += OnBotKickedFromChannel;
            newIrc.DisconnectedEvent += (sender, args1) => Stop();

            // initialise the deferred installers.
            container.Install(new DeferredInstaller());
        }

        private static void OnBotKickedFromChannel(object sender, KickedEventArgs e)
        {
            // FIXME: service locator
            var channelRepository = ServiceLocator.Current.GetInstance<IChannelRepository>();

            var channel = channelRepository.GetByName(e.Channel);
            channel.Enabled = false;
            channelRepository.Save(channel);
        }

        /// <summary>
        /// The IRC invite event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void IrcInviteEvent(object sender, InviteEventArgs e)
        {
            var legacyUser = LegacyUser.NewFromOtherUser(e.User);

            if (legacyUser == null)
            {
                throw new NullReferenceException(string.Format("Legacy user creation failed from user {0}", e.User));
            }

            // FIXME: ServiceLocator - CSH
            new Join(
                legacyUser,
                e.Nickname,
                new[] { e.Channel },
                ServiceLocator.Current.GetInstance<ICommandServiceHelper>()).RunCommand();
        }

        /// <summary>
        /// The notify on join event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void NotifyOnJoinEvent(object sender, JoinEventArgs e)
        {
            try
            {
                // FIXME: ServiceLocator - CSH
                var commandServiceHelper = ServiceLocator.Current.GetInstance<ICommandServiceHelper>();

                var legacyUser = LegacyUser.NewFromOtherUser(e.User);
                if (legacyUser == null)
                {
                    throw new NullReferenceException(string.Format("Legacy user creation failed from user {0}", e.User));
                }

                new Notify(legacyUser, e.Channel, new string[0], commandServiceHelper).NotifyJoin(legacyUser, e.Channel);
            }
            catch (Exception exception)
            {
                Log.Error("Exception encountered in NotifyOnJoinEvent", exception);
            }
        }

        /// <summary>
        /// The received message.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="ea">
        /// The new event args.
        /// </param>
        /// <remarks>
        /// TODO: upgrade this, get rid of PMEA call.
        /// </remarks>
        private static void ReceivedMessage(object sender, MessageReceivedEventArgs ea)
        {
            if (ea.Message.Command != "PRIVMSG")
            {
                return;
            }

            var parameters = ea.Message.Parameters.ToList();

            string message = parameters[1];

            var cmd = new LegacyCommandParser(
                ServiceLocator.Current.GetInstance<ICommandServiceHelper>(),
                Log.CreateChildLogger("LegacyCommandParser"),
                ServiceLocator.Current.GetInstance<IRedirectionParserService>(),
                ServiceLocator.Current.GetInstance<BotConfiguration>());
            try
            {
                bool overrideSilence = cmd.OverrideBotSilence;
                if (cmd.IsRecognisedMessage(ref message, ref overrideSilence, (IIrcClient)sender))
                {
                    cmd.OverrideBotSilence = overrideSilence;
                    string[] messageWords = message.Split(' ');
                    string command = messageWords[0].ToLowerInvariant();
                    string joinedargs = string.Join(" ", messageWords, 1, messageWords.Length - 1);
                    string[] commandArgs = joinedargs == string.Empty ? new string[0] : joinedargs.Split(' ');

                    cmd.HandleCommand(LegacyUser.NewFromString(ea.Message.Prefix), parameters[0], command, commandArgs);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// The join channels.
        /// </summary>
        private static void JoinChannels()
        {
            // FIXME: ServiceLocator - channelrepo
            var channelRepository = ServiceLocator.Current.GetInstance<IChannelRepository>();
            
            foreach (var channel in channelRepository.GetEnabled())
            {
                newIrc.JoinChannel(channel.Name);
            }
        }
    }
}
