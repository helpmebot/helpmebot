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

        public ModuleLoader(IList<string> moduleList)
        {
            this.moduleList = moduleList;
        }

        public void LoadModules(IWindsorContainer container)
        {
            foreach (var module in this.moduleList)
            {
                var assembly = Assembly.LoadFile(Path.GetFullPath(module));
                container.Install(FromAssembly.Instance(assembly));
            }
        }
    }
}