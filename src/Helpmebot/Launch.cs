namespace Helpmebot
{
    using System;
    using System.IO;
    using System.Net;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Startup;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;

    public class Launch
    {
        private static void Main(string[] args)
        {
            // get the path to the configuration file
            string configurationFile = "configuration.xml";

            if (args.Length >= 1)
            {
                configurationFile = args[0];
            }

            if (!File.Exists(configurationFile))
            {
                var fullPath = Path.GetFullPath(configurationFile);

                Console.WriteLine("Configuration file at {0} does not exist!", fullPath);
                return;
            }

            // setup the container
            var container = new WindsorContainer("modules.xml");

            // Load other module assemblies, and add them to the relevant installation queues
            var moduleLoader = container.Resolve<ModuleLoader>();
            moduleLoader.LoadModules();
            
            // import the configuration
            container.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(configurationFile));
            
            // post-configuration, pre-initialisation actions
            ConfigureCertificateValidation(container);
            
            // install into the container
            container.Install(new MainInstaller());
            moduleLoader.InstallModules(container);
            container.Release(moduleLoader);

            var application = container.Resolve<IApplication>();
            application.Run();

            container.Release(application);
            container.Dispose();
        }

        internal static void ConfigureCertificateValidation(IWindsorContainer container)
        {
            var disableCertificateValidation = container.Resolve<BotConfiguration>().DisableCertificateValidation;

            if (disableCertificateValidation)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            }
        }
    }
}