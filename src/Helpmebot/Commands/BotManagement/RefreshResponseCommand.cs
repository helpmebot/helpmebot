namespace Helpmebot.Commands.BotManagement
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("refresh")]
    [CommandFlag(Flags.BotManagement)]
    public class RefreshResponseCommand : CommandBase
    {
        private readonly IMessageService messageService;

        public RefreshResponseCommand(
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

        protected override IEnumerable<CommandResponse> Execute()
        {
            this.messageService.RefreshResponseRepository();

            yield return new CommandResponse
            {
                Message = "Done."
            };
        }
    }
}