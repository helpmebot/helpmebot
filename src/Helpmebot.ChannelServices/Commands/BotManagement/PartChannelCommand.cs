namespace Helpmebot.ChannelServices.Commands.BotManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("part")]
    [CommandFlag(Flags.BotManagement, true)]
    [CommandFlag(Flags.LocalConfiguration)]
    [HelpSummary("Leave an IRC channel")]
    public class PartChannelCommand : CommandBase
    {
        private readonly IChannelManagementService channelManagementService;
        private readonly IResponder responder;

        public PartChannelCommand(
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

        [Help("[channel]", "Leaves the specified channel, or the current channel if no channel is specified.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var channel = this.CommandSource;

            if (this.Arguments.Any())
            {
                channel = this.Arguments.First();
            }
            
            if (!channel.StartsWith("#"))
            {
                throw new CommandErrorException(channel + " is not a valid channel");
            }

            var partMessage = this.responder.GetMessagePart(
                "channelservices.command.part.requested-by",
                this.CommandSource,
                this.User.ToString());
            
            this.channelManagementService.PartChannel(channel, partMessage);
                
            yield break;
        }
    }
}