namespace Helpmebot.CoreServices.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Startup;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Services;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("version")]
    [CommandFlag(Flags.Standard)]
    [HelpCategory("Diagnostics")]
    public class VersionCommand : CommandBase
    {
        public static string BotVersion
        {
            get
            {
                var mainAssembly = Assembly.GetAssembly(Type.GetType("Helpmebot.Launch, Helpmebot"));
                var version = mainAssembly.GetName().Version;
                
                return string.Format(
                    "{0}.{1} (build {2})",
                    version.Major.ToString(CultureInfo.InvariantCulture),
                    version.Minor.ToString(CultureInfo.InvariantCulture),
                    version.Build.ToString(CultureInfo.InvariantCulture)
                );
            }
        }
        
        private readonly ModuleLoader moduleLoader;

        public VersionCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ModuleLoader moduleLoader) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.moduleLoader = moduleLoader;
        }

        [Help("[--modules]", "Provides the current version of the bot and the key libraries")]
            
        protected override IEnumerable<CommandResponse> Execute()
        {
            var ircVersion = this.GetFileVersion(Assembly.GetAssembly(typeof(IrcClient)));
            var botLibVersion = this.GetFileVersion(Assembly.GetAssembly(typeof(CommandHandler)));
            var mediaWikiLibVersion = this.GetFileVersion(Assembly.GetAssembly(typeof(MediaWikiApi)));

            var messageFormat =
                "Version {0}; using Stwalkerster.IrcClient v{3}, Stwalkerster.Bot.CommandLib v{4}, Stwalkerster.Bot.MediaWikiLib v{5} (Runtime: {6} / {7})";
            var message = string.Format(
                messageFormat,
                BotVersion,
                "",
                "",
                ircVersion,
                botLibVersion,
                mediaWikiLibVersion,
                RuntimeInformation.FrameworkDescription,
                Environment.OSVersion
            );

            yield return new CommandResponse { Message = message };

            if (this.Arguments.Contains("--modules"))
            {
                foreach (var moduleVersion in
                    this.moduleLoader.LoadedAssemblies
                        .Select(assembly => $"{assembly.GetName().Name} (v{this.GetFileVersion(assembly)})")
                        .Select(x => new CommandResponse { Message = x }))
                {
                    yield return moduleVersion;
                }
            }
        }

        private string GetFileVersion(Assembly assembly)
        {
            return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        }
    }
}