namespace Helpmebot.ChannelServices.Startup
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Helpmebot.ChannelServices.Services;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Commands.Interfaces;

    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyContaining<Installer>()
                    .InNamespace("Helpmebot.ChannelServices.Services")
                    .WithServiceAllInterfaces(),
                Classes.FromAssemblyContaining<Installer>().BasedOn<ICommand>().LifestyleTransient()
            );
        }
    }
}