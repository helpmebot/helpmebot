namespace Helpmebot.Commands.Commands.Diagnostics
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Extensions;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flag.Owner)]
    [CommandInvocation("rawctcp")]
    [HelpCategory("Diagnostics")]
    public class RawCtcpCommand : CommandBase
    {
        public RawCtcpCommand(
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

        [Help("<CTCP command> <destination> [content]", "Sends a CTCP command to the specified destination")]
        [RequiredArguments(2)]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var command = this.Arguments.First();
            var destination = this.Arguments.Skip(1).First();
            var content = string.Join(" ", this.Arguments.Skip(2));

            this.Client.SendMessage(destination, content.SetupForCtcp(command));

            yield break;
        }
    }
}