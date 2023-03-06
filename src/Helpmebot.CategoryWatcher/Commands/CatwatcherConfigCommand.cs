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
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
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
        
        
        [SubcommandInvocation("configure")]
        [SubcommandInvocation("config")]
        [RequiredArguments(1)]
        [Help("<flag>")]
        [CommandFlag(Flags.Configuration, true)]
        [CommandFlag(Flags.LocalConfiguration)]
        [CommandParameter("enable", "Enable reporting in this channel", "enable", typeof(bool))]
        [CommandParameter("disable", "Disable reporting in this channel", "enable", typeof(bool), booleanInverse:true)]
        [CommandParameter("interval=", "Sets the interval (in seconds) between reporting category members to the channel", "interval", typeof(string))]
        [CommandParameter("show-wait-time", "Show the wait time of each item", "showWait", typeof(bool))]
        [CommandParameter("hide-wait-time", "Hide the wait time of each item", "showWait", typeof(bool), booleanInverse: true)]
        [CommandParameter("min-wait-time=", "Set the minimum wait time of an item before the wait time is shown (if enabled)", "minWait", typeof(string))]
        [CommandParameter("show-links", "Show the URL to each item as well as the wikilink", "showLink", typeof(bool))]
        [CommandParameter("hide-links", "Hide the URL to each item, showing only the wikilink", "showLink", typeof(bool), booleanInverse: true)]
        [CommandParameter("show-additions", "Report additions to the category as they are detected", "showAdditions", typeof(bool))]
        [CommandParameter("hide-additions", "Don't report additions", "showAdditions", typeof(bool), booleanInverse: true)]
        [CommandParameter("show-removals", "Report removals from the category as soon as they are detected", "showRemovals", typeof(bool))]
        [CommandParameter("hide-removals", "Don't report removals", "showRemovals", typeof(bool), booleanInverse: true)]
        protected IEnumerable<CommandResponse> ConfigureMode()
        {
            if (this.CommandSource == this.Client.Nickname)
            {
                return this.responder.Respond("catwatcher.command.catwatcher.configure.not-via-pm", this.CommandSource);
            }
            
            var watcher = this.Arguments.First();

            if (!this.catWatcherConfig.GetValidWatcherKeys().Contains(watcher))
            {
                return this.responder.Respond(
                    "catwatcher.command.catwatcher.configure.non-existent",
                    this.CommandSource);
            }

            var enabled = this.Parameters.GetParameter("enable", (bool?)null);
            var interval = this.Parameters.GetParameter("interval", (string)null);
            var showWaitTime = this.Parameters.GetParameter("showWait", (bool?)null);
            var minWait = this.Parameters.GetParameter("minWait", (string)null);
            var showLink = this.Parameters.GetParameter("showLink", (bool?)null);
            var showAdditions = this.Parameters.GetParameter("showAdditions", (bool?)null);
            var showRemovals = this.Parameters.GetParameter("showRemovals", (bool?)null);

            var config = this.catWatcherConfig.GetWatcherConfiguration(watcher, this.CommandSource, true);

            bool changed = false;
            
            if (enabled.HasValue)
            {
                config.Enabled = enabled.Value;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(interval) && int.TryParse(interval, out var intervalValue))
            {
                config.SleepTime = intervalValue;
                changed = true;
            }

            if (showWaitTime.HasValue)
            {
                config.ShowWaitTime = showWaitTime.Value;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(minWait) && int.TryParse(minWait, out var minWaitValue))
            {
                config.MinWaitTime = minWaitValue;
                changed = true;
            }

            if (showLink.HasValue)
            {
                config.ShowLink = showLink.Value;
                changed = true;
            }

            if (showAdditions.HasValue)
            {
                config.AlertForAdditions = showAdditions.Value;
                changed = true;
            }

            if (showRemovals.HasValue)
            {
                config.AlertForRemovals = showRemovals.Value;
                changed = true;
            }

            if (changed)
            {
                this.catWatcherConfig.SaveWatcherConfiguration(config);
                return this.responder.Respond("common.done", this.CommandSource);
            }
            
            return this.responder.Respond("catwatcher.command.catwatcher.configure.not-changed", this.CommandSource);
        }
        
        
    }
}