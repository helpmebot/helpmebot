namespace Helpmebot.CoreServices.Startup
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
    using Helpmebot.Configuration.Startup;
    using Helpmebot.CoreServices.Facilities;
    using Helpmebot.CoreServices.Services.AccessControl;
    using Helpmebot.TypedFactories;
    using Stwalkerster.Bot.CommandLib.Commands.Interfaces;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Services;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;
    using Installer = Stwalkerster.IrcClient.Installer;

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
                Component.For<IFlagService>().ImplementedBy<AccessControlAuthorisationService>(),

                // Startup
                Component.For<IApplication>().ImplementedBy<Launcher>(),

                // Registration by convention
                Classes.FromAssemblyContaining<MainInstaller>().BasedOn<ICommand>().LifestyleTransient(),
                Classes.FromAssemblyContaining<MainInstaller>().InNamespace("Helpmebot.Repositories").WithService.AllInterfaces(),
                Classes.FromAssemblyContaining<MainInstaller>().InNamespace("Helpmebot.Services").WithService.AllInterfaces(),
                Classes.FromAssemblyContaining<MainInstaller>().InNamespace("Helpmebot.Services.AccessControl").WithService.AllInterfaces(),
                Classes.FromAssemblyContaining<MainInstaller>().InNamespace("Helpmebot.Background").WithService.AllInterfaces(),

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