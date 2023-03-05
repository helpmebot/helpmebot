namespace Helpmebot.CategoryWatcher.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Attributes;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using CoreServices.Model;
    using CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Configuration;
    using Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;
    
    [CommandInvocation("catwatcher")]
    [CommandFlag(Flags.Protected)]
    [HelpCategory("CatWatcher")]
    [HelpSummary("Manages category watcher configuration")]
    public class CatwatcherConfigCommand : CommandBase
    {
        private readonly IWatcherConfigurationService catWatcherConfig;
        private readonly IItemPersistenceService itemPersistenceService;
        private readonly MediaWikiSiteConfiguration wikiConfiguration;
        private readonly ICategoryWatcherHelperService categoryWatcherHelperService;
        private readonly ICategoryWatcherBackgroundService backgroundService;
        private readonly IResponder responder;

        public CatwatcherConfigCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IWatcherConfigurationService catWatcherConfig,
            IItemPersistenceService itemPersistenceService,
            MediaWikiSiteConfiguration wikiConfiguration,
            ICategoryWatcherHelperService categoryWatcherHelperService,
            ICategoryWatcherBackgroundService backgroundService,
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
            this.catWatcherConfig = catWatcherConfig;
            this.itemPersistenceService = itemPersistenceService;
            this.wikiConfiguration = wikiConfiguration;
            this.categoryWatcherHelperService = categoryWatcherHelperService;
            this.backgroundService = backgroundService;
            this.responder = responder;
        }

        [SubcommandInvocation("list")]
        [Help("", "Lists all configured category watchers")]
        protected IEnumerable<CommandResponse> ListMode()
        {
            var responses = new List<CommandResponse>
            {
                new CommandResponse
                {
                    Message = this.responder.GetMessagePart(
                        "catwatcher.command.catwatcher.list.start",
                        this.CommandSource)
                }
            };
            
            var watchers = this.catWatcherConfig.GetWatchers()
                .Select(
                    x => this.responder.GetMessagePart(
                        "catwatcher.command.catwatcher.list.item",
                        this.CommandSource,
                        new object[] { x.Keyword, x.Category }))
                .Select(x => new CommandResponse { Message = x });
            responses.AddRange(watchers);

            return responses;
        }

        [SubcommandInvocation("create")]
        [RequiredArguments(3)]
        [Help("<flag> <wiki> <category>")]
        [CommandFlag(Flags.Configuration, true)]
        protected IEnumerable<CommandResponse> CreateMode()
        {
            var flag = this.Arguments[0];
            var wiki = this.Arguments[1];
            var category = string.Join(" ", this.Arguments.Skip(2));

            if (flag == "default")
            {
                throw new CommandErrorException("Cannot use 'default' as a category watcher keyword.");
            }

            if (this.wikiConfiguration.Sites.All(x => x.WikiId != wiki))
            {
                throw new CommandErrorException("No configuration for this wiki exists.");
            }

            if (this.catWatcherConfig.GetWatchers().Any(x => x.Keyword == flag))
            {
                throw new CommandErrorException("This category watcher keyword already exists.");
            }

            var watcher = this.catWatcherConfig.CreateWatcher(category, flag, wiki);
            this.categoryWatcherHelperService.RegisterWatcher(watcher);

            return this.responder.Respond("common.done", this.CommandSource);
        }
        
        [SubcommandInvocation("delete")]
        [RequiredArguments(1)]
        [Help("<flag>")]
        [CommandFlag(Flags.Configuration, true)]
        protected IEnumerable<CommandResponse> DeleteMode()
        {
            this.backgroundService.Suspend();
            try
            {
                var flag = this.Arguments[0];

                if (this.catWatcherConfig.GetWatchers().All(x => x.Keyword != flag))
                {
                    return this.responder.Respond(
                        "catwatcher.command.catwatcher.delete.non-existent",
                        this.CommandSource);
                }

                var channelsForWatcher = this.catWatcherConfig.GetChannelsForWatcher(flag).ToList();
                var deconfigure = false;
                if (channelsForWatcher.Contains(this.CommandSource))
                {
                    deconfigure = true;
                    channelsForWatcher.Remove(this.CommandSource);
                }

                if (channelsForWatcher.Any())
                {
                    return this.responder.Respond("catwatcher.command.catwatcher.delete.in-use", this.CommandSource);
                }

                if (deconfigure)
                {
                    // Deconfigure the current channel
                    // FIXME: deconfigure the watcher from this channel
                }

                // Remove the watcher command
                this.categoryWatcherHelperService.DeregisterWatcher(flag);

                // Purge the cache of category items
                var items = this.itemPersistenceService.GetItems(flag);
                this.itemPersistenceService.RemoveDeletedItems(flag, items.Select(x => x.Title));

                // Finally delete the watcher itself
                this.catWatcherConfig.DeleteWatcher(flag);

                return this.responder.Respond("common.done", this.CommandSource);
            }
            finally
            {
                this.backgroundService.Resume();
            }
        }
    }
}