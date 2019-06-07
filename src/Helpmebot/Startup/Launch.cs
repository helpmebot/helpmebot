namespace Helpmebot.Startup
{
    using System;
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Castle.Core.Logging;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Helpmebot.Model;
    using Microsoft.Practices.ServiceLocation;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;

    public class Launch : IApplication
    {
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly ICommandParser commandParser;
        private readonly CommandOverrideConfiguration commandOverrideConfiguration;
        private readonly ISession globalSession;
        private readonly ManualResetEvent exitLock;
        
        public DateTime StartupTime { get; private set; }

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
            var container = new WindsorContainer(configurationFile);

            // post-configuration, pre-initialisation actions
            ConfigureCertificateValidation(container);
            
            // set up the service locator
            // TODO: remove me
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
            
            // install into the container
            container.Install(new MainInstaller());

            var application = container.Resolve<IApplication>();
            application.Run();
            
            container.Release(application);
            container.Dispose();
        }

        public Launch(
            ILogger logger,
            IIrcClient client,
            ICommandParser commandParser,
            CommandOverrideConfiguration commandOverrideConfiguration,
            ISession globalSession)
        {
            this.logger = logger;
            this.client = client;
            this.commandParser = commandParser;
            this.commandOverrideConfiguration = commandOverrideConfiguration;
            this.globalSession = globalSession;

            this.StartupTime = DateTime.Now;

            this.exitLock = new ManualResetEvent(false);

            this.client.DisconnectedEvent += (sender, args) => this.Stop();
        }
        
        public void Stop()
        {
            this.exitLock.Set();
        }

        public void Run()
        {
            // process command overrides
            foreach (var mapEntry in this.commandOverrideConfiguration.OverrideMap)
            {
                this.commandParser.UnregisterCommand(mapEntry.Keyword, mapEntry.Channel);
                this.commandParser.RegisterCommand(mapEntry.Keyword, mapEntry.Type, mapEntry.Channel);
            }

            using (var tx = this.globalSession.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var channels = this.globalSession.CreateCriteria<Channel>().Add(Restrictions.Eq("Enabled", true)).List<Channel>();
                tx.Rollback();
                
                // join the necessary channels
                foreach (var channel in channels)
                {
                    this.client.JoinChannel(channel.Name);
                }
            }
            
            this.logger.Info("Awaiting exit signal.");
            this.exitLock.WaitOne();
            this.logger.Error("Exit signal received!");
        }
        
        internal static void ConfigureCertificateValidation(IWindsorContainer container)
        {
            var disableCertificateValidation = container.Resolve<BotConfiguration>().DisableCertificateValidation;
            
            if (disableCertificateValidation)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            }
        }
    }
}