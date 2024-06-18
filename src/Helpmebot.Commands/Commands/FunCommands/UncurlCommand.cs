namespace Helpmebot.Commands.Commands.FunCommands
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Uncurl)]
    [CommandInvocation("uncurl")]
    [HelpSummary("Enables all fun commands in the current channel.")]
    public class UncurlCommand : CommandBase
    {
        private readonly IChannelManagementService channelManagementService;
        private readonly IResponder responder;

        public UncurlCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IChannelManagementService channelManagementService,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.channelManagementService = channelManagementService;
            this.responder = responder;
        }

        protected override IEnumerable<CommandResponse> Execute()
        {
            try
            {
                this.channelManagementService.ConfigureFunCommands(this.CommandSource, false);
                return this.responder.Respond("funcommands.command.uncurl", this.CommandSource, this.CommandSource);
            }
            catch (NullReferenceException)
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
            }
        }
    }
}