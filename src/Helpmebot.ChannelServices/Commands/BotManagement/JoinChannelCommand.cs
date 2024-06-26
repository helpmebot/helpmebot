namespace Helpmebot.ChannelServices.Commands.BotManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("join")]
    [CommandFlag(Flags.BotManagement, true)]
    [HelpSummary("Joins the specified channel")]
    public class JoinChannelCommand : CommandBase
    {
        private readonly IChannelManagementService channelManagementService;

        public JoinChannelCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.channelManagementService = channelManagementService;
        }

        [RequiredArguments(1)]
        [Help("<channel>", "This command is also usable through an /invite on IRC.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var channelName = this.Arguments.First();

            if (channelName == "0")
            {
                throw new CommandInvocationException();
            }
            
            if (!channelName.StartsWith("#"))
            {
                throw new CommandErrorException(channelName + " is not a valid channel");
            }

            this.channelManagementService.JoinChannel(channelName);

            yield break;
        }
    }
}