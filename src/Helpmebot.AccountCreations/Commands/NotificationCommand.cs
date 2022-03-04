namespace Helpmebot.AccountCreations.Commands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.AccountCreations.Services.Interfaces;
    using Helpmebot.Attributes;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("notifications")]
    [CommandFlag(Flags.LocalConfiguration)]
    [CommandFlag(Flags.Configuration, true)]    
    [HelpSummary("Manages AMQP notification delivery to the channel.")]
    [HelpCategory("ACC")]
    public class NotificationCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IMqNotificationService notificationService;

        public NotificationCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
            IMqNotificationService notificationService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
            this.notificationService = notificationService;
        }

        [SubcommandInvocation("enable")]
        [Help("", "Enables delivery of notifications to this channel. For notifications to actually arrive, they must also be sent to the bot via AMQP.")]
        protected IEnumerable<CommandResponse> EnableCommand()
        {
            if (this.notificationService.Active)
            {
                this.notificationService.Bind(this.CommandSource);
                return this.responder.Respond("accountcreations.command.notifications.enabled", this.CommandSource);
            }
            else
            {
                throw new CommandErrorException(this.responder.GetMessagePart("accountcreations.command.notifications.unavailable", this.CommandSource));
            }
        }

        [SubcommandInvocation("disable")]
        [Help("", "Disables delivery of notifications to this channel")]
        protected IEnumerable<CommandResponse> DisableCommand()
        {
            if (this.notificationService.Active)
            {
                this.notificationService.Unbind(this.CommandSource);
                return this.responder.Respond("accountcreations.command.notifications.disabled", this.CommandSource);
            }
            else
            {
                throw new CommandErrorException(this.responder.GetMessagePart("accountcreations.command.notifications.unavailable", this.CommandSource));
            }
        }
    }
}