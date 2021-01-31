namespace Helpmebot.Commands.Commands.BotInfo
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("ping")]
    [CommandFlag(Flags.BotInfo)]
    public class PingCommand : CommandBase
    {
        private readonly IMessageService messageService;

        public PingCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMessageService messageService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.messageService = messageService;
        }

        [Help("[username]", "Replies to a ping with a pong")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            string name;
            string message;

            if (this.Arguments.Count == 0)
            {
                name = this.User.Nickname;
                message = this.messageService.RetrieveMessage("cmdPing", this.CommandSource, new[] { name });
            }
            else
            {
                name = string.Join(" ", this.Arguments);
                message = this.messageService.RetrieveMessage("cmdPingUser", this.CommandSource, new[] { name });
            }

            yield return new CommandResponse {Message = message};
        }
    }
}