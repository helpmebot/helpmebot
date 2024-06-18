namespace Helpmebot.CoreServices.Startup
{
    using System;
    using System.IO;
    using Castle.Facilities.Logging;
    using Castle.Services.Logging.Log4netIntegration;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Security;
    using Helpmebot.CoreServices.Services.Geolocation;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Component = Castle.MicroKernel.Registration.Component;

    public class Entrypoint
    {
        public static void MainEntrypoint(string[] args)
        {
            // get the path to the configuration file
            string configurationFile = "Configuration/configuration.yml";

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
            var container = new WindsorContainer();

            var globalConfiguration = ConfigurationReader.ReadConfiguration<GlobalConfiguration>(configurationFile);
            container.AddFacility<LoggingFacility>(f => f.LogUsing<Log4netFactory>().WithConfig(globalConfiguration.General.Log4NetConfiguration));
            container.Register(Component.For<ModuleLoader>());
            
            // Load other module assemblies, and add them to the relevant installation queues            
            var moduleLoader = container.Resolve<ModuleLoader>(new {moduleList = globalConfiguration.Modules});
            moduleLoader.LoadModuleAssemblies();

            new CommandOverrideMapEntryInflater().Inflate(globalConfiguration.CommandOverrides);
            
            container.Register(
                Component.For<IIrcConfiguration>().Instance(globalConfiguration.Irc.ToConfiguration()),
                Component.For<BotConfiguration>().Instance(globalConfiguration.General),
                Component.For<DatabaseConfiguration>().Instance(globalConfiguration.Database),
                Component.For<WikimediaUrlShortnerConfiguration>().Instance(globalConfiguration.WikimediaShortener),
                Component.For<CommandOverrideConfiguration>().Instance(globalConfiguration.CommandOverrides),
                Component.For<RabbitMqConfiguration>().Instance(globalConfiguration.MqConfiguration),
                Component.For<MediaWikiSiteConfiguration>().Instance(globalConfiguration.MediaWikiSites ?? new MediaWikiSiteConfiguration()),
                Component.For<IMessageRepository, DatabaseMessageRepository>().ImplementedBy<DatabaseMessageRepository>(),
                Component.For<IMessageRepository, FileMessageRepository>().ImplementedBy<FileMessageRepository>().Named("fileMessageRepository")
            );

            SetupGeolocation(globalConfiguration, container);
            SetupUrlShortener(globalConfiguration, container);
            
            moduleLoader.InstallModuleConfiguration(container);
            
            // post-configuration, pre-initialisation actions
            TransportLayerSecurityConfigurationProvider.ConfigureCertificateValidation(globalConfiguration.General.DisableCertificateValidation);
            
            // install into the container
            container.Install(new MainInstaller());
            moduleLoader.InstallModuleServices(container);
            container.Release(moduleLoader);

            var application = container.Resolve<IApplication>();
            application.Run();

            container.Release(application);
            container.Dispose();
        }

        private static void SetupUrlShortener(GlobalConfiguration globalConfiguration, WindsorContainer container)
        {
            var primary = Type.GetType(globalConfiguration.General.UrlShortener);
            var secondary = globalConfiguration.General.SecondaryUrlShortener != null
                ? Type.GetType(globalConfiguration.General.SecondaryUrlShortener)
                : null;

            container.Register(
                Component.For<IUrlShorteningService>().ImplementedBy(primary),
                Component.For<IUrlShorteningService>().ImplementedBy(secondary).Named("secondaryShortener")
            );
        }

        private static void SetupGeolocation(GlobalConfiguration globalConfiguration, WindsorContainer container)
        {
            if (globalConfiguration.General.MaxMindDatabasePath != null)
            {
                container.Register(Component.For<IGeolocationService>().ImplementedBy<MaxMindGeolocationService>());
            }
            else if (globalConfiguration.General.IpInfoDbApiKey != null)
            {
                container.Register(Component.For<IGeolocationService>().ImplementedBy<IpInfoDbGeolocationService>());
            }
            else
            {
                container.Register(Component.For<IGeolocationService>().ImplementedBy<FakeGeolocationService>());
            }
        }
    }
}