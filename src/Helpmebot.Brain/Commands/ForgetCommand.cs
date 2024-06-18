namespace Helpmebot.Brain.Commands
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.Attributes;
    using Helpmebot.Brain.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Brain)]
    [CommandInvocation("forget")]
    [CommandInvocation("unlearn")]
    [HelpCategory("Brain")]
    [HelpSummary("Removes the provided keywords from the learnt command list.")]
    public class ForgetCommand : CommandBase
    {
        private readonly IKeywordService keywordService;
        private readonly IResponder responder;

        public ForgetCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IKeywordService keywordService,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.keywordService = keywordService;
            this.responder = responder;
        }

        [RequiredArguments(1)]
        [Help("<keyword> [keyword...]", new[]{"Provide the keyword(s) to remove as parameters.", "At least one keyword is required."})]
        protected override IEnumerable<CommandResponse> Execute()
        {
            foreach (var argument in this.Arguments)
            {
                this.keywordService.Delete(argument);
            }
            
            return this.responder.Respond("brain.command.forget", this.CommandSource);
        }
    }
}
