namespace Helpmebot.Brain.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.Brain.Services.Interfaces;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Protected)]
    [CommandInvocation("brainsearch")]
    [CommandInvocation("bs")]   
    [HelpSummary("Searches for a brain entry by content, returning the keywords of any matching entries. If there are few enough results, the full content of those entries will also be returned.")]
    [HelpCategory("Brain")]

    public class BrainSearchCommand : CommandBase
    {
        private readonly IKeywordService keywordService;
        private readonly IResponder responder;

        public BrainSearchCommand(
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
        [Help("<search...>", "The text to search for in all brain entries")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var search = string.Join(" ", this.Arguments).ToLowerInvariant();  
            
            // FIXME: this is really inefficient...
            var all = this.keywordService.GetAll();
            var results = all.Where(x => x.Response.ToLowerInvariant().Contains(search) || x.Name.ToLowerInvariant().Contains(search)).ToList();

            if (results.Count == 0)
            {
                return this.responder.Respond(
                    "brain.command.search.no-results",
                    this.CommandSource,
                    search);
            }
            
            if (results.Count > 5)
            {
                return this.responder.Respond(
                    "brain.command.search.too-many-results",
                    this.CommandSource,
                    HttpUtility.UrlEncode(search));
            }

            var responses = new List<CommandResponse>(results.Count + 2);

            var nameList = string.Join(", ", results.Select(x => x.Name));

            responses.AddRange(
                this.responder.Respond(
                    "brain.command.search.result-list",
                    this.CommandSource,
                    new object[] { nameList, results.Count },
                    ignoreRedirection: true));
            
            foreach (var r in results)
            {
                var messagePart = this.responder.GetMessagePart(
                    "brain.command.search.result",
                    this.CommandSource,
                    new object[] { r.Name, r.Response });
                
                responses.Add(
                    new CommandResponse
                    {
                        Message = messagePart,
                        Destination = CommandResponseDestination.PrivateMessage,
                        IgnoreRedirection = true,
                        Type = CommandResponseType.Notice
                    });
            }
            
            return responses;
        }
    }
}