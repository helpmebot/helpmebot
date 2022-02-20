namespace Helpmebot.CoreServices.Startup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Castle.Core.Logging;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Stwalkerster.Bot.CommandLib.Commands.Interfaces;

    public class ModuleLoader
    {
        private readonly List<LoadableModuleConfiguration> moduleList;
        private readonly ILogger logger;
        public List<Assembly> LoadedAssemblies { get; } = new List<Assembly>();

        public ModuleLoader(List<LoadableModuleConfiguration> moduleList, ILogger logger)
        {
            this.moduleList = moduleList;
            this.logger = logger;
        }

        internal void LoadModuleAssemblies()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName()).ToList();

            var filesFound = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .ToDictionary(x => AssemblyName.GetAssemblyName(x).FullName);

            foreach (var module in this.moduleList)
            {
                this.logger.InfoFormat("Loading module {0}", module.Assembly);
                var assembly = Assembly.LoadFile(Path.GetFullPath(module.Assembly));
                // allAssemblies.Add(assembly.GetName());
                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    if (allAssemblies.Any(x => x.FullName == referencedAssembly.FullName))
                    {
                        continue;
                    }

                    try
                    {
                        this.logger.DebugFormat("Loading module {0} dependency {1}", module.Assembly, referencedAssembly.Name);
                        Assembly.Load(referencedAssembly);
                    }
                    catch (FileNotFoundException) when (filesFound.ContainsKey(referencedAssembly.FullName))
                    {
                        this.logger.DebugFormat(
                            "Loading module {0} dependency {1} from file {2}",
                            module.Assembly,
                            referencedAssembly.Name,
                            filesFound[referencedAssembly.FullName]);
                        
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
                    this.logger.DebugFormat("Loading configuration for {0} from {1}", module.Assembly, file.File);
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
                var assemblyNamespace = assembly.FullName.Split(',')[0];
                
                this.logger.DebugFormat("Installing services for {0}", assemblyNamespace);
                
                container.Register(
                    Classes.FromAssembly(assembly).BasedOn<ICommand>().LifestyleTransient(),            
                    Classes.FromAssembly(assembly).InNamespace(assemblyNamespace + ".Services").WithServiceAllInterfaces()
                );
            }
        }
    }
}