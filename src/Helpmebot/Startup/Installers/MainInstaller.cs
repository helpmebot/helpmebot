namespace Helpmebot.Startup.Installers
{
    using Castle.Facilities.EventWiring;
    using Castle.Facilities.Logging;
    using Castle.Facilities.Startable;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Services.Logging.Log4netIntegration;
    using Castle.Windsor;
    using Helpmebot.Commands;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Services;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Startup.Facilities;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;

    public class MainInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<LoggingFacility>(f => f.LogUsing<Log4netFactory>().WithConfig("logger.config"));
            container.AddFacility<PersistenceFacility>();
            container.AddFacility<EventWiringFacility>();

            container.Register(
                // Legacy stuff
                Component.For<ILegacyDatabase>().ImplementedBy<LegacyDatabase>(),
                Component.For<LegacyConfig>().ImplementedBy<LegacyConfig>(),
                Component.For<ICommandServiceHelper>().ImplementedBy<CommandServiceHelper>(),

                // Registration by convention
                Classes.FromThisAssembly().InNamespace("Helpmebot.Repositories").WithService.AllInterfaces(),
                Classes.FromThisAssembly().InNamespace("Helpmebot.Services").WithService.AllInterfaces(),

                // IRC client
                Component.For<ISupportHelper>().ImplementedBy<SupportHelper>(),
                Component
                    .For<IIrcClient>()
                    .ImplementedBy<IrcClient>()
                    .Start()
                    .PublishEvent(
                        p => p.JoinReceivedEvent += null,
                        x => x
                            .To<JoinMessageService>(l => l.OnJoinEvent(null, null))
                            .To<BlockMonitoringService>(l => l.OnJoinEvent(null, null)))
            );
        }
    }
}