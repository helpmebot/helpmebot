namespace Helpmebot.Configuration
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    public class ModuleLoader
    {
        private readonly IList<string> moduleList;
        private readonly List<Assembly> loadedAssemblies = new List<Assembly>();
        
        public ModuleLoader(IList<string> moduleList)
        {
            this.moduleList = moduleList;
        }

        public void LoadModules()
        {
            foreach (var module in this.moduleList)
            {
                var assembly = Assembly.LoadFile(Path.GetFullPath(module));
                this.loadedAssemblies.Add(assembly);
            }
        }

        public void InstallModuleConfiguration(IWindsorContainer container)
        {
            var botConfiguration = container.Resolve<BotConfiguration>();

            foreach (var module in this.moduleList)
            {
                var configPath = Path.GetFileName(module).Replace(".dll", ".config.xml");
                if (!string.IsNullOrWhiteSpace(botConfiguration.ModuleConfigurationPath))
                {
                    configPath = botConfiguration.ModuleConfigurationPath + Path.DirectorySeparatorChar + configPath;
                }
                
                container.Install(Configuration.FromXmlFile(configPath));
            }
        }
        
        public void InstallModules(IWindsorContainer container)
        {
            foreach (var assembly in this.loadedAssemblies)
            {
                container.Install(FromAssembly.Instance(assembly));
            }
        }
    }
}