namespace Helpmebot.ChannelServices.Commands.BotManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("part")]
    [CommandFlag(Flags.BotManagement)]
    [CommandFlag(Flags.LocalConfiguration)]
    public class PartChannelCommand : CommandBase
    {
        private readonly ISession session;
        private readonly IChannelManagementService channelManagementService;
        private readonly IMessageService messageService;

        public PartChannelCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession session,
            IChannelManagementService channelManagementService,
            IMessageService messageService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.session = session;
            this.channelManagementService = channelManagementService;
            this.messageService = messageService;
        }

        [Help(new[]{"","<channel>"}, new[]{"Leaves the current channel", "Leaves the specified channel"})]
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

            var partMessage = this.messageService.RetrieveMessage(
                Messages.RequestedBy,
                this.CommandSource,
                new[] {this.User.ToString()});
            
            this.channelManagementService.PartChannel(channel, this.session, partMessage);
                
            yield break;
        }
    }
}