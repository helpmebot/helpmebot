namespace Helpmebot.CategoryWatcher.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.CategoryWatcher.Services.Interfaces;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("catwatcherignored")]
    [CommandFlag(Flags.Protected)]
    [HelpCategory("CatWatcher")]
    [HelpSummary("Manages pages ignored by the category watcher")]
    public class CatwatcherIgnoredConfigCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IItemPersistenceService itemPersistenceService;

        public CatwatcherIgnoredConfigCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
            IItemPersistenceService itemPersistenceService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
            this.itemPersistenceService = itemPersistenceService;
        }

        [SubcommandInvocation("list")]
        [Help("", "Lists all pages ignored by category watchers")]
        protected IEnumerable<CommandResponse> ListMode()
        {
            var responses = new List<CommandResponse>
            {
                new CommandResponse
                {
                    Message = this.responder.GetMessagePart(
                        "catwatcher.command.catwatcherignored.list.start",
                        this.CommandSource)
                }
            };

            var ignoredPages = this.itemPersistenceService.GetIgnoredPages()
                .Select(
                    x => this.responder.GetMessagePart(
                        "catwatcher.command.catwatcherignored.list.item",
                        this.CommandSource,
                        new object[] { x }))
                .Select(x => new CommandResponse { Message = x });
            responses.AddRange(ignoredPages);

            return responses;
        }

        [SubcommandInvocation("add")]
        [Help("<page>", "Adds a page to the category watcher ignore list")]
        [RequiredArguments(1)]
        [CommandFlag(Flags.Configuration, true)]
        protected IEnumerable<CommandResponse> AddMode()
        {
            var page = string.Join(" ", this.Arguments);

            this.itemPersistenceService.AddIgnoredPage(page);

            return this.responder.Respond("catwatcher.command.catwatcherignored.added", this.CommandName, page);
        }

        [SubcommandInvocation("remove")]
        [Help("<page>", "Removes a page from the category watcher ignore list")]
        [RequiredArguments(1)]
        [CommandFlag(Flags.Configuration, true)]
        protected IEnumerable<CommandResponse> RemoveMode()
        {
            var page = string.Join(" ", this.Arguments);

            var done = this.itemPersistenceService.RemoveIgnoredPage(page);

            if (done)
            {
                return this.responder.Respond("catwatcher.command.catwatcherignored.removed", this.CommandName, page);
            }

            return this.responder.Respond("catwatcher.command.catwatcherignored.notfound", this.CommandName, page);
        }
    }
}