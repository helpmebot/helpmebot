namespace Helpmebot.Configuration
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Windows.Input;
    using Castle.MicroKernel.Registration;
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
                var configFileName = Path.GetFileName(module).Replace(".dll", ".config.xml");
                var configPath = "Configuration" + Path.DirectorySeparatorChar + configFileName;
                if (!string.IsNullOrWhiteSpace(botConfiguration.ModuleConfigurationPath))
                {
                    var customConfigPath = botConfiguration.ModuleConfigurationPath + Path.DirectorySeparatorChar + configFileName;

                    if (File.Exists(customConfigPath))
                    {
                        configPath = customConfigPath;
                    }
                }
                
                container.Install(Configuration.FromXmlFile(configPath));
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