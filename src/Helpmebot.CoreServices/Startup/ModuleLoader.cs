namespace Helpmebot.CoreServices.Startup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Stwalkerster.Bot.CommandLib.Commands.Interfaces;

    public class ModuleLoader
    {
        private readonly List<ModuleConfiguration> moduleList;
        public List<Assembly> LoadedAssemblies { get; } = new List<Assembly>();

        public ModuleLoader(List<ModuleConfiguration> moduleList)
        {
            this.moduleList = moduleList;
        }

        internal void LoadModuleAssemblies()
        {
            foreach (var module in this.moduleList)
            {
                var assembly = Assembly.LoadFile(Path.GetFullPath(module.Assembly));
                this.LoadedAssemblies.Add(assembly);
            }
        }
        
        internal void InstallModuleConfiguration(IWindsorContainer container)
        {
            foreach (var module in this.moduleList.Where(x => x.Configuration != null))
            {
                foreach (var file in module.Configuration)
                {
                    var target = Type.GetType(file.Type);
                    var config = ConfigurationReader.ReadConfiguration(file.File, target);
                    container.Register(Component.For(target).Instance(config));
                }
            }
        }
        
        internal void InstallModuleServices(IWindsorContainer container)
        {
            foreach (var assembly in this.LoadedAssemblies)
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