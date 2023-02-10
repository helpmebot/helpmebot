namespace Helpmebot.Commands.Commands.Information
{
    using System.Collections.Generic;
    using System.Web;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("google")]
    [CommandFlag(Flags.Info)]
    [HelpSummary("Returns a message with a link to a google search result")]
    public class GoogleCommand : CommandBase
    {
        private readonly IResponder responder;

        public GoogleCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder
            ) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
        }

        [RequiredArguments(1)]
        [Help("<search>")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var arguments = HttpUtility.UrlEncode(string.Join(" ", this.Arguments));
            return this.responder.Respond("commands.command.google", this.CommandSource, arguments);
        }
    }
}