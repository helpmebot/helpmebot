namespace Helpmebot.CoreServices.Startup
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Threading;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Prometheus;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Services;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;

    public class Launcher : IApplication
    {
        public static readonly DateTime StartupTime = DateTime.UtcNow;

        private static readonly Gauge VersionInfo = Metrics.CreateGauge(
            "helpmebot_build_info",
            "Build info",
            new GaugeConfiguration
            {
                LabelNames = new[]
                    {"assembly", "irclib", "commandlib", "mediawikilib", "runtime", "os", "targetFramework"}
            });

        private static readonly Gauge StartupTimeMetric = Metrics.CreateGauge(
            "helpmebot_startup_time_seconds",
            "Start time of the process");
        
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly ICommandParser commandParser;
        private readonly CommandOverrideConfiguration commandOverrideConfiguration;
        private readonly IChannelManagementService channelManagementService;
        private readonly ManualResetEvent exitLock;

        public Launcher(
            ILogger logger,
            IIrcClient client,
            ICommandParser commandParser,
            ICommandHandler commandHandler,
            CommandOverrideConfiguration commandOverrideConfiguration,
            BotConfiguration botConfiguration,
            IChannelManagementService channelManagementService)
        {
            
            this.logger = logger;
            this.client = client;
            this.commandParser = commandParser;
            this.commandOverrideConfiguration = commandOverrideConfiguration;
            this.channelManagementService = channelManagementService;

            StartupTimeMetric.Set((StartupTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
            
            this.exitLock = new ManualResetEvent(false);

            this.client.DisconnectedEvent += (sender, args) => this.Stop();
            this.client.ReceivedMessage += commandHandler.OnMessageReceived;

            if (botConfiguration.PrometheusMetricsPort.HasValue)
            {
                var metricsServer = new MetricServer(botConfiguration.PrometheusMetricsPort.Value);
                metricsServer.Start();

                var mainAssembly = Assembly.GetAssembly(Type.GetType("Helpmebot.Launch, Helpmebot"));
                
                VersionInfo.WithLabels(
                        FileVersionInfo.GetVersionInfo(mainAssembly.Location).FileVersion,
                        FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(IrcClient)).Location)
                            .FileVersion,
                        FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(CommandBase)).Location)
                            .FileVersion,
                        FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(MediaWikiApi)).Location)
                            .FileVersion,
                        RuntimeInformation.FrameworkDescription,
                        Environment.OSVersion.ToString(),
                        ((TargetFrameworkAttribute) mainAssembly
                            .GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
                            .FirstOrDefault())?.FrameworkDisplayName ?? "Unknown"
                    )
                    .Set(1);
            }
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
                this.commandParser.RegisterCommand(mapEntry.Keyword, mapEntry.CommandType, mapEntry.Channel);
            }
            
            // join the necessary channels
            var channels = this.channelManagementService.GetEnabledChannels();
            foreach (var channel in channels)
            {
                this.client.JoinChannel(channel);
            }
            
            this.logger.Info("Awaiting exit signal.");
            this.exitLock.WaitOne();
            this.logger.Error("Exit signal received!");
        }

    }
}