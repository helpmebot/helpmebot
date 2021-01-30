namespace Helpmebot.Commands.Information
{
    using System.Collections.Generic;
    using System.Web;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("google")]
    [CommandFlag(Flags.Info)]
    public class GoogleCommand : CommandBase
    {
        private readonly IMessageService messageService;

        public GoogleCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMessageService messageService
            ) : base(
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

        [RequiredArguments(1)]
        [Help("<search>", "Returns a message with a link to a google search result")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var arguments = HttpUtility.UrlEncode(string.Join(" ", this.Arguments));
            var response = this.messageService.RetrieveMessage(
                "google-response",
                this.CommandSource,
                new[] {arguments});

            yield return new CommandResponse
            {
                Message = response
            };
        }
    }
}