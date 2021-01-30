namespace Helpmebot.Configuration
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    public class ModuleLoader
    {
        private readonly IList<string> earlyModuleList;
        private readonly IList<string> moduleList;
        private readonly List<Assembly> loadedAssemblies = new List<Assembly>();
        private readonly List<Assembly> loadedEarlyAssemblies = new List<Assembly>();
        public ModuleLoader(IList<string> earlyModuleList, IList<string> moduleList)
        {
            this.earlyModuleList = earlyModuleList;
            this.moduleList = moduleList;
        }

        public void LoadModules()
        {
            foreach (var module in this.earlyModuleList)
            {
                var assembly = Assembly.LoadFile(Path.GetFullPath(module));
                this.loadedEarlyAssemblies.Add(assembly);
            }
            
            foreach (var module in this.moduleList)
            {
                var assembly = Assembly.LoadFile(Path.GetFullPath(module));
                this.loadedAssemblies.Add(assembly);
            }
        }
        
        public void InstallEarlyModules(IWindsorContainer container)
        {
            foreach (var assembly in this.loadedEarlyAssemblies)
            {
                container.Install(FromAssembly.Instance(assembly));
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