namespace Helpmebot.CoreServices.Startup
{
    using Castle.Facilities.Startable;
    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Facilities;
    using Helpmebot.CoreServices.Services.AccessControl;
    using Helpmebot.TypedFactories;
    using Microsoft.Extensions.Logging;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.Interfaces;
    using Stwalkerster.Bot.CommandLib.Services;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.TypedFactories;
    using Stwalkerster.Bot.MediaWikiLib.Services;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;

    using IrcClient = Stwalkerster.IrcClient.IrcClient;
    
    public class MainInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var botConfiguration = container.Resolve<BotConfiguration>();
            var loggerFactory = new LoggerFactory().AddLog4Net(botConfiguration.Log4NetConfiguration);
            
            container.AddFacility<PersistenceFacility>();
            container.AddFacility<StartableFacility>(f => f.DeferredStart());
            container.AddFacility<TypedFactoryFacility>();

            container.Register(
                // CommandParser
                Component.For<ILogger<CommandParser>>().UsingFactoryMethod(loggerFactory.CreateLogger<CommandParser>),
                Component.For<ILogger<CommandHandler>>().UsingFactoryMethod(loggerFactory.CreateLogger<CommandHandler>),
                Classes.FromAssemblyContaining<CommandBase>().BasedOn<ICommand>().LifestyleTransient(),
                Component.For<ICommandTypedFactory>().AsFactory(),
                Classes.FromAssemblyContaining<CommandParser>().InSameNamespaceAs<CommandParser>().WithServiceAllInterfaces(),
                
                // Legacy stuff
                Component.For<IFlagService>().ImplementedBy<AccessControlAuthorisationService>(),

                // Startup
                Component.For<IApplication>().ImplementedBy<Launcher>(),

                // Registration by convention
                Classes.FromAssemblyContaining<MainInstaller>().BasedOn<ICommand>().LifestyleTransient(),
                Classes.FromAssemblyContaining<MainInstaller>().InNamespace("Helpmebot.CoreServices.Services").WithService.AllInterfaces(),
                Classes.FromAssemblyContaining<MainInstaller>().InNamespace("Helpmebot.CoreServices.Services.AccessControl").WithService.AllInterfaces(),
                Classes.FromAssemblyContaining<MainInstaller>().InNamespace("Helpmebot.CoreServices.Services.Messages").WithService.AllInterfaces(),
                Classes.FromAssemblyContaining<MainInstaller>().InNamespace("Helpmebot.CoreServices.Background").WithService.AllInterfaces(),

                // MediaWiki API stuff
                Component.For<IMediaWikiApiTypedFactory>().AsFactory(),
                Component.For<IMediaWikiApi>().ImplementedBy<MediaWikiApi>().LifestyleTransient(),
                Component.For<IWebServiceClient>().ImplementedBy<WebServiceClient>(),

                // IRC client
                Component.For<ILoggerFactory>().Instance(loggerFactory),
                Component.For<ILogger<SupportHelper>>().UsingFactoryMethod(loggerFactory.CreateLogger<SupportHelper>),
                Component.For<ISupportHelper>().ImplementedBy<SupportHelper>(),
                Component.For<IIrcClient>().ImplementedBy<IrcClient>().Start()
            );
        }
    }
}