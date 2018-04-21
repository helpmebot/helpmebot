﻿namespace Helpmebot.Startup
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Castle.Core.Logging;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Helpmebot.Repositories.Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Interfaces;

    // TODO: replace me with lib interface
    public interface IApplication
    {
        void Stop();
        void Run();
    }
    
    public class Launch : IApplication
    {
        [Obsolete("Move me elsewhere at the earliest opportunity")]
        public static readonly DateTime StartupTime = DateTime.Now;
        
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly IChannelRepository channelRepository;
        private readonly ManualResetEvent exitLock;

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
        
        public Launch(ILogger logger, IIrcClient client, IChannelRepository channelRepository)
        {
            this.logger = logger;
            this.client = client;
            this.channelRepository = channelRepository;

            this.exitLock = new ManualResetEvent(false);

            this.client.DisconnectedEvent += (sender, args) => this.Stop();
        }
        
        public void Stop()
        {
            this.exitLock.Set();
        }

        public void Run()
        {
            foreach (var channel in this.channelRepository.GetEnabled())
            {
                this.client.JoinChannel(channel.Name);
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