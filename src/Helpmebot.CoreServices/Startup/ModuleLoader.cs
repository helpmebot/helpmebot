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
        private readonly List<LoadableModuleConfiguration> moduleList;
        public List<Assembly> LoadedAssemblies { get; } = new List<Assembly>();

        public ModuleLoader(List<LoadableModuleConfiguration> moduleList)
        {
            this.moduleList = moduleList;
        }

        internal void LoadModuleAssemblies()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName()).ToList();

            var filesFound = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .ToDictionary(x => AssemblyName.GetAssemblyName(x).FullName);

            foreach (var module in this.moduleList)
            {
                var assembly = Assembly.LoadFile(Path.GetFullPath(module.Assembly));
                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    if (allAssemblies.Contains(referencedAssembly))
                    {
                        continue;
                    }

                    try
                    {
                        Assembly.Load(referencedAssembly);
                    }
                    catch (FileNotFoundException ex) when (filesFound.ContainsKey(referencedAssembly.FullName))
                    {
                        Assembly.LoadFile(filesFound[referencedAssembly.FullName]);
                    }

                    allAssemblies.Add(referencedAssembly);
                }
                
                this.LoadedAssemblies.Add(assembly);
            }
        }
        
        internal void InstallModuleConfiguration(IWindsorContainer container)
        {
            foreach (var module in this.moduleList.Where(x => x.Configuration != null))
            {
                foreach (var file in module.Configuration)
                {
                    var target = TypeResolver.GetType(file.Type);
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