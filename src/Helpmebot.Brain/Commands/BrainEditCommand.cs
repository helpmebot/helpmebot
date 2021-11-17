namespace Helpmebot.Brain.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.Brain.Services.Interfaces;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;
    
    [CommandFlag(Flags.Brain)]
    [CommandInvocation("brainedit")]
    [CommandInvocation("editbrain")]
    [HelpSummary("Edits an existing brain entry by applying a sed-style expression to a message.")]
    [HelpCategory("Brain")]
    public class BrainEditCommand : CommandBase
    {
        private readonly IKeywordService keywordService;
        private readonly ISedExpressionService sedExpressionService;
        private readonly IResponder responder;

        public BrainEditCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IKeywordService keywordService,
            ISedExpressionService sedExpressionService,
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
            this.sedExpressionService = sedExpressionService;
            this.responder = responder;
        }

        [RequiredArguments(2)]
        [Help("<keyword> <expression>", new[]
        {
            "Applies the expression to the content of the specified learnt command, and returns the result",
            "Expressions should be in GNU sed substitution format ( s/search/replace/ ), but using .NET-flavour regular expressions. The flags g and i are supported for global matches and case insensitive matches respectively.",
            "For backreferences in the search pattern, use the \\1 format. For backreferences in the replacement pattern, use $1 format."
        })]
        [SubcommandInvocation("test")]
        [SubcommandInvocation("dryrun")]
        protected IEnumerable<CommandResponse> Test()
        {
            var keyword = this.DoReplacement(out var result);

            var responses = this.responder.Respond(
                    "brain.command.edit.proposal",
                    this.CommandSource,
                    new object[] { keyword.Name },
                    destination: CommandResponseDestination.PrivateMessage,
                    type: CommandResponseType.Notice,
                    ignoreRedirection: true)
                .ToList();
            
            responses.Add(new CommandResponse
            {
                IgnoreRedirection = true,
                Destination = CommandResponseDestination.PrivateMessage,
                Message = result,
                ClientToClientProtocol = keyword.Action ? "ACTION" : null
            });
            
            return responses;
        }
        
        [RequiredArguments(2)]
        [Help("<keyword> <expression>", "Applies the expression to the content of the specified learnt command as 'test' subcommand, and saves the change to the message.")]
        [SubcommandInvocation("edit")]
        protected IEnumerable<CommandResponse> Edit()
        {
            var keyword = this.DoReplacement(out var result);
            var trigger = keyword.Name;
            var action = keyword.Action;

            this.keywordService.Delete(keyword.Name);
            this.keywordService.Create(trigger, result, action);

            return this.responder.Respond(
                "brain.command.edit.completed",
                this.CommandSource,
                new object[] { keyword.Name },
                destination: CommandResponseDestination.PrivateMessage,
                type: CommandResponseType.Notice,
                ignoreRedirection: true);
        }

        private Keyword DoReplacement(out string result)
        {
            var trigger = this.Arguments.First();
            var expression = string.Join(" ", this.Arguments.Skip(1));

            var keyword = this.keywordService.Get(trigger);
            if (keyword == null)
            {
                throw new CommandErrorException("Keyword not found");
            }

            result = this.sedExpressionService.Apply(keyword.Response, expression);
            return keyword;
        }
    }
}