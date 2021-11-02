namespace Helpmebot.CoreServices.Startup
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;
    using Helpmebot.Configuration;
    using Stwalkerster.Bot.CommandLib.Commands.Interfaces;

    public class ModuleLoader
    {
        private readonly List<ModuleConfiguration> moduleList;
        private readonly List<Assembly> loadedAssemblies = new List<Assembly>();
        
        public ModuleLoader(List<ModuleConfiguration> moduleList)
        {
            this.moduleList = moduleList;
        }

        public void LoadModuleAssemblies()
        {
            foreach (var module in this.moduleList)
            {
                var assembly = Assembly.LoadFile(Path.GetFullPath(module.Assembly));
                this.loadedAssemblies.Add(assembly);
            }
        }

        public void InstallModuleCastleFiles(IWindsorContainer container)
        {
            var botConfiguration = container.Resolve<BotConfiguration>();

            foreach (var module in this.moduleList.Where(x => !string.IsNullOrWhiteSpace(x.CastleFile)))
            {
                container.Install(Configuration.FromXmlFile(module.CastleFile));
            }
        }
        
        public void InstallModules(IWindsorContainer container)
        {
            foreach (var assembly in this.loadedAssemblies)
            {
                var ns = assembly.FullName.Split(',')[0];
                
                container.Register(
                    Classes.FromAssembly(assembly).BasedOn<ICommand>().LifestyleTransient(),            
                    Classes.FromAssembly(assembly).InNamespace(ns + ".Services").WithServiceAllInterfaces()
                );
            }
        }
    }
}