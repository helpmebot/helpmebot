namespace Helpmebot.Startup
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
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Services;
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
            container.AddFacility<StartableFacility>(f => f.DeferredStart());
            
            // Chainload other installers.
            container.Install(new Installer());
            
            container.Register(
                // Legacy stuff
                Component.For<ILegacyDatabase>().ImplementedBy<LegacyDatabase>(),
                Component.For<ICommandServiceHelper>().ImplementedBy<CommandServiceHelper>(),
                Component.For<ILegacyCommandHandler>().ImplementedBy<LegacyCommandHandler>(),

                // Startup 
                Component.For<IApplication>().ImplementedBy<Launch>(),

                // Registration by convention
                Classes.FromThisAssembly().InNamespace("Helpmebot.Repositories").WithService.AllInterfaces(),
                Classes.FromThisAssembly().InNamespace("Helpmebot.Services").WithService.AllInterfaces(),
                Classes.FromThisAssembly().InNamespace("Helpmebot.Background").WithService.AllInterfaces(),

                // IRC client
                Component
                    .For<IIrcClient>()
                    .ImplementedBy<IrcClient>()
                    .Start()
                    .PublishEvent(
                        p => p.InviteReceivedEvent += null,
                        x => x.To<ChannelManagementService>(l => l.OnInvite(null, null)))
                    .PublishEvent(
                        p => p.JoinReceivedEvent += null,
                        x => x
                            .To<JoinMessageService>(l => l.OnJoinEvent(null, null))
                            .To<BlockMonitoringService>(l => l.OnJoinEvent(null, null)))
                    .PublishEvent(
                        p => p.ReceivedMessage += null,
                        x => x
                            .To<LinkerService>(l => l.IrcPrivateMessageEvent(null, null))
                            .To<LegacyCommandHandler>(l => l.ReceivedMessage(null, null))
                       )
                    .PublishEvent(
                        p => p.WasKickedEvent += null,
                        x => x.To<ChannelManagementService>(l => l.OnKicked(null, null)))
            );
        }
    }
}