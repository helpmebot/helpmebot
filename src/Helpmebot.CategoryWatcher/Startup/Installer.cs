namespace Helpmebot.CategoryWatcher.Startup
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Stwalkerster.Bot.CommandLib.Commands.Interfaces;

    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyContaining<Installer>()
                    .InNamespace("Helpmebot.CategoryWatcher.Services")
                    .WithServiceAllInterfaces(),
                Classes.FromAssemblyContaining<Installer>().BasedOn<ICommand>().LifestyleTransient()
            );
        }
    }
}