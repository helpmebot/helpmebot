namespace Helpmebot.Startup
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Threading;
    using Castle.Core.Logging;
    using Castle.Windsor;
    using Helpmebot.Configuration;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Prometheus;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Services;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;

    public class Launch : IApplication
    {
        private static readonly Gauge VersionInfo = Metrics.CreateGauge(
            "helpmebot_build_info",
            "Build info",
            new GaugeConfiguration
            {
                LabelNames = new[] {"assembly", "irclib", "commandlib", "mediawikilib", "runtime", "os", "targetFramework"}
            });

        private static readonly Gauge StartupTimeMetric = Metrics.CreateGauge(
            "helpmebot_startup_time_seconds",
            "Start time of the process");
        
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly ICommandParser commandParser;
        private readonly CommandOverrideConfiguration commandOverrideConfiguration;
        private readonly ISession globalSession;
        private readonly ManualResetEvent exitLock;
        
        private static DateTime startupTime;

        public DateTime StartupTime => startupTime;

        /// <summary>
        /// The main.
        /// </summary>
        private static void Main(string[] args)
        {
            startupTime = DateTime.Now;
            
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
            var container = new WindsorContainer(configurationFile);

            // post-configuration, pre-initialisation actions
            ConfigureCertificateValidation(container);
            
            // install into the container
            container.Install(new MainInstaller());

            MetricServer metricsServer;
            var botConfiguration = container.Resolve<BotConfiguration>();
            if (botConfiguration.PrometheusMetricsPort.HasValue)
            {
                metricsServer = new MetricServer(botConfiguration.PrometheusMetricsPort.Value);
                metricsServer.Start();
                
                VersionInfo.WithLabels(
                        FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion,
                        FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(IrcClient)).Location)
                            .FileVersion,
                        FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(CommandBase)).Location)
                            .FileVersion,
                        FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(MediaWikiApi)).Location)
                            .FileVersion,
                        Environment.Version.ToString(),
                        Environment.OSVersion.ToString(),
                        ((TargetFrameworkAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(TargetFrameworkAttribute),false).FirstOrDefault())?.FrameworkDisplayName ?? "Unknown"        
                    )
                    .Set(1);
            }

            var application = container.Resolve<IApplication>();
            application.Run();
            
            container.Release(application);
            container.Dispose();
        }

        public Launch(
            ILogger logger,
            IIrcClient client,
            ICommandParser commandParser,
            CommandOverrideConfiguration commandOverrideConfiguration,
            ISession globalSession)
        {
            this.logger = logger;
            this.client = client;
            this.commandParser = commandParser;
            this.commandOverrideConfiguration = commandOverrideConfiguration;
            this.globalSession = globalSession;

            StartupTimeMetric.Set((this.StartupTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
            
            this.exitLock = new ManualResetEvent(false);

            this.client.DisconnectedEvent += (sender, args) => this.Stop();
        }
        
        public void Stop()
        {
            this.exitLock.Set();
        }

        public void Run()
        {
            // process command overrides
            foreach (var mapEntry in this.commandOverrideConfiguration.OverrideMap)
            {
                this.commandParser.UnregisterCommand(mapEntry.Keyword, mapEntry.Channel);
                this.commandParser.RegisterCommand(mapEntry.Keyword, mapEntry.Type, mapEntry.Channel);
            }

            using (var tx = this.globalSession.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var channels = this.globalSession.CreateCriteria<Channel>().Add(Restrictions.Eq("Enabled", true)).List<Channel>();
                tx.Rollback();
                
                // join the necessary channels
                foreach (var channel in channels)
                {
                    this.client.JoinChannel(channel.Name);
                }
            }
            
            this.logger.Info("Awaiting exit signal.");
            this.exitLock.WaitOne();
            this.logger.Error("Exit signal received!");
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