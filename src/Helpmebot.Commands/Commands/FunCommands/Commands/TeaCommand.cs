namespace Helpmebot.Commands.Commands.FunCommands.Commands
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.Commands.Commands.FunCommands;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("tea")]
    [CommandFlag(Flags.Fun)]
    [HelpSummary("Brew us a cuppa!")]
    public class TeaCommand : TargetedFunCommandBase
    {
        public TeaCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client,
            responder,
            channelManagementService)
        {
        }

        [Help("[user]")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            return this.GetResponse("funcommands.command.tea");
        }
    }
}