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
    using Castle.Core.Logging;
    using Castle.Windsor;
    using helpmebot6.Commands;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Startup;
    using Helpmebot.Startup.Installers;
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
        /// The main.
        /// </summary>
        private static void Main(string[] args)
        {
            // get the path to the configuration file
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
            
            // setup the container
            container = new WindsorContainer(configurationFile);
            
            // post-configuration, pre-initialisation actions
            Launch.ConfigureCertificateValidation(container);
            
            // set up the service locator
            // TODO: remove me
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
            
            // install into the container
            container.Install(new MainInstaller());

            /////////////////////////////////////////////////
            //// TODO: REMOVE THIS ENTIRE BLOCK
            /////////////////////////////////////////////////
            {
                Log = ServiceLocator.Current.GetInstance<ILogger>();
                Log.Info("Initialising Helpmebot...");

                newIrc = container.Resolve<IIrcClient>();

                newIrc.JoinReceivedEvent += NotifyOnJoinEvent;

                // initialise the deferred installers.
                container.Install(new DeferredInstaller());
            }
            /////////////////////////////////////////////////
            /////////////////////////////////////////////////
            /////////////////////////////////////////////////

            var application = container.Resolve<IApplication>();
            application.Run();
            
            container.Release(application);
            container.Dispose();
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
    }
}
