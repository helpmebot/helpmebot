namespace Helpmebot.CoreServices.Startup
{
    using System;
    using System.IO;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Security;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Component = Castle.MicroKernel.Registration.Component;

    public class Entrypoint
    {
        public static void MainEntrypoint(string[] args)
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
            var container = new WindsorContainer();

            // Load other module assemblies, and add them to the relevant installation queues
            var globalConfiguration = ConfigurationReader.ReadConfiguration<GlobalConfiguration>(configurationFile.Replace(".xml", ".yml"));
            var moduleLoader = new ModuleLoader(globalConfiguration.Modules);
            
            moduleLoader.LoadModuleAssemblies();
            
            container.Register(
                Component.For<IIrcConfiguration>().Instance(globalConfiguration.Irc.ToConfiguration()),
                Component.For<BotConfiguration>().Instance(globalConfiguration.General),
                Component.For<DatabaseConfiguration>().Instance(globalConfiguration.Database),
                Component.For<MediaWikiDocumentationConfiguration>().Instance(globalConfiguration.Documentation)
            );
            
            // import the configuration
            container.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(configurationFile));
            
            moduleLoader.InstallModuleCastleFiles(container);
            moduleLoader.InstallModuleConfiguration(container);
            
            // post-configuration, pre-initialisation actions
            TransportLayerSecurityConfigurationProvider.ConfigureCertificateValidation(globalConfiguration.General.DisableCertificateValidation);
            
            // install into the container
            container.Install(new MainInstaller());
            moduleLoader.InstallModuleServices(container);
            container.Release(moduleLoader);

            var application = container.Resolve<IApplication>();
            application.Run();

            container.Release(application);
            container.Dispose();
        }
    }
}