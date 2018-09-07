namespace Helpmebot.Commands.Diagnostics
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Services;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("version")]
    [CommandFlag(Flag.Standard)]
    public class VersionCommand : CommandBase
    {
        public VersionCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
        }

        [Help("", "Provides the current version of the bot and the key libraries")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var ircVersion = this.GetFileVersion(Assembly.GetAssembly(typeof(IrcClient)));
            var botLibVersion = this.GetFileVersion(Assembly.GetAssembly(typeof(CommandHandler)));

            var messageFormat =
                "Version {0}.{1} (Build {2}); using Stwalkerster.IrcClient v{3}, Stwalkerster.Bot.CommandLib v{4}";
            var message = string.Format(
                messageFormat,
                version.Major.ToString(CultureInfo.InvariantCulture),
                version.Minor.ToString(CultureInfo.InvariantCulture),
                version.Build.ToString(CultureInfo.InvariantCulture),
                ircVersion,
                botLibVersion
            );

            yield return new CommandResponse
            {
                Message = message
            };
        }

        private string GetFileVersion(Assembly assembly)
        {
            return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        }
    }
}