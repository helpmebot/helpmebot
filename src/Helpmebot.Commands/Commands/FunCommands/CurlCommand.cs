namespace Helpmebot.Commands.Commands.FunCommands
{
    using System;
    using System.Collections.Generic;
    using Castle.Core.Logging;
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

    [CommandFlag(Flags.Protected)]
    [CommandInvocation("curl")]
    [HelpSummary("Disables all fun commands in the current channel.")]
    public class CurlCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public CurlCommand(
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
            client)
        {
            this.responder = responder;
            this.channelManagementService = channelManagementService;
        }
        
        protected override IEnumerable<CommandResponse> Execute()
        {
            try
            {
                this.channelManagementService.ConfigureFunCommands(this.CommandSource, true);
                return this.responder.Respond("funcommands.command.curl", this.CommandSource, this.CommandSource);
            }
            catch (NullReferenceException)
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
            }
        }
    }
}