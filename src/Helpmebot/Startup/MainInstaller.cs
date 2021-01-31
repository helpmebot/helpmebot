namespace Helpmebot.Startup
{
    using Castle.Facilities.Logging;
    using Castle.Facilities.Startable;
    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.MicroKernel.SubSystems.Conversion;
    using Castle.Services.Logging.Log4netIntegration;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Helpmebot.Startup.Converters;
    using Helpmebot.Startup.Facilities;
    using Helpmebot.TypedFactories;
    using Stwalkerster.Bot.CommandLib.Commands.Interfaces;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Services;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;

    public class MainInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var botConfiguration = container.Resolve<BotConfiguration>();
            botConfiguration.Log4NetConfiguration = botConfiguration.Log4NetConfiguration ?? "logger.config";
            
            container.AddFacility<LoggingFacility>(
                f => f.LogUsing<Log4netFactory>().WithConfig(botConfiguration.Log4NetConfiguration));
            container.AddFacility<PersistenceFacility>();
            container.AddFacility<StartableFacility>(f => f.DeferredStart());
            container.AddFacility<TypedFactoryFacility>();

            // Configuration converters
            var conversionManager =
                (IConversionManager) container.Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);
            conversionManager.Add(new CommandOverrideMapEntryConverter());

            // Chainload other installers.
            container.Install(
                new Installer(),
                new Stwalkerster.Bot.CommandLib.Startup.Installer()
            );

            container.Register(
                // Legacy stuff
                Component.For<IFlagService>().ImplementedBy<Helpmebot.CoreServices.Services.AccessControl.AccessControlAuthorisationService>(),

                // Startup
                Component.For<IApplication>().ImplementedBy<Launch>(),

                // Registration by convention
                Classes.FromThisAssembly().BasedOn<ICommand>().LifestyleTransient(),
                Classes.FromThisAssembly().InNamespace("Helpmebot.Repositories").WithService.AllInterfaces(),
                Classes.FromThisAssembly().InNamespace("Helpmebot.Services").WithService.AllInterfaces(),
                Classes.FromThisAssembly().InNamespace("Helpmebot.Services.AccessControl").WithService.AllInterfaces(),
                Classes.FromThisAssembly().InNamespace("Helpmebot.Background").WithService.AllInterfaces(),

                // MediaWiki API stuff
                Component.For<IMediaWikiApiTypedFactory>().AsFactory(),
                Component.For<IMediaWikiApi>().ImplementedBy<MediaWikiApi>().LifestyleTransient(),
                Component.For<IWebServiceClient>().ImplementedBy<WebServiceClient>(),

                // IRC client
                Component.For<IIrcClient>().ImplementedBy<IrcClient>().Start()
            );

            container.Resolve<ModuleLoader>().InstallModules(container);
        }
    }
}