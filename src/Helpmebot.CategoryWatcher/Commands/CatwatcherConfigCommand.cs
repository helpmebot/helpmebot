namespace Helpmebot.CategoryWatcher.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Attributes;
    using Microsoft.Extensions.Logging;
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
        [Help("<flag> <wiki> <category>", "Adds a category watcher with the specified flag, on the specified wiki shortcode, watching the specified category.")]
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
        [Help("<flag>", new[]{"Removes a category watcher entirely.","The category watcher must be disabled in every channel before it can be deleted."})]
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

                var categoryWatcherChannels = this.catWatcherConfig
                    .GetChannelsForWatcher(flag)
                    .Select(x => this.catWatcherConfig.GetWatcherConfiguration(flag, x))
                    .Where(x => x != null)
                    .ToList();
                
                if (categoryWatcherChannels.Any(x => x.Enabled))
                {
                    return this.responder.Respond("catwatcher.command.catwatcher.delete.in-use", this.CommandSource);
                }

                if (categoryWatcherChannels.Any())
                {
                    this.catWatcherConfig.DeleteWatcherConfiguration(flag);
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
        [CommandParameter("e|enable", "Enable reporting in this channel", "enable", typeof(bool))]
        [CommandParameter("d|disable", "Disable reporting in this channel", "enable", typeof(bool), booleanInverse:true)]
        [CommandParameter("i|interval=", "Sets the interval (in seconds) between reporting category members to the channel", "interval", typeof(string))]
        [CommandParameter("w|show-wait-time", "Show the wait time of each item", "showWait", typeof(bool))]
        [CommandParameter("W|hide-wait-time", "Hide the wait time of each item", "showWait", typeof(bool), booleanInverse: true)]
        [CommandParameter("m|min-wait-time=", "Set the minimum wait time of an item before the wait time is shown (if enabled)", "minWait", typeof(string))]
        [CommandParameter("l|show-links", "Show the URL to each item as well as the wikilink", "showLink", typeof(bool))]
        [CommandParameter("L|hide-links", "Hide the URL to each item, showing only the wikilink", "showLink", typeof(bool), booleanInverse: true)]
        [CommandParameter("a|show-additions", "Report additions to the category as they are detected", "showAdditions", typeof(bool))]
        [CommandParameter("A|hide-additions", "Don't report additions", "showAdditions", typeof(bool), booleanInverse: true)]
        [CommandParameter("r|show-removals", "Report removals from the category as soon as they are detected", "showRemovals", typeof(bool))]
        [CommandParameter("R|hide-removals", "Don't report removals", "showRemovals", typeof(bool), booleanInverse: true)]
        [CommandParameter("s|statusmsg=", "Configure message target within channel. Use `@` to only send to channel operators, or `+` to only send to voiced users. Use `all` to send to everyone.", "statusmsg", typeof(string))]
        [CommandParameter("t|anchor=", "Configure a specific anchor on the page to link to. For example, set to `potato` for link to `https://enwp.org/User_talk:Example#potato`. To clear, use an empty parameter (eg, `--anchor=`", "anchor", typeof(string))]
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
            var statusMsg = this.Parameters.GetParameter("statusmsg", (string)null);
            var anchor = this.Parameters.GetParameter("anchor", (string)null);

            var config = this.catWatcherConfig.GetWatcherConfiguration(watcher, this.CommandSource, true);

            var changed = false;
            var timerChanged = false;
            
            if (enabled.HasValue)
            {
                config.Enabled = enabled.Value;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(interval) && int.TryParse(interval, out var intervalValue))
            {
                config.SleepTime = intervalValue;
                timerChanged = true;
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

            if (!string.IsNullOrWhiteSpace(statusMsg))
            {
                if (statusMsg.ToLower() == "all")
                {
                    config.StatusMsg = null;
                    changed = true;
                }
                else
                {
                    if (this.Client.StatusMsgDestinationFlags.Contains(statusMsg))
                    {
                        config.StatusMsg = statusMsg;
                        changed = true;
                    }
                    else
                    {
                        return this.responder.Respond(
                            "catwatcher.command.catwatcher.configure.unknown-statusmsg",
                            this.CommandSource,
                            new object[] { statusMsg, string.Join("", this.Client.StatusMsgDestinationFlags) });
                    }
                }
            }

            if (anchor != null)
            {
                if (string.IsNullOrEmpty(anchor))
                {
                    config.Anchor = null;
                    changed = true;
                }
                else
                {
                    config.Anchor = anchor;
                    changed = true;
                }
            }

            if (changed)
            {
                this.catWatcherConfig.SaveWatcherConfiguration(config);

                if (timerChanged)
                {
                    this.backgroundService.ResetChannelTimer(config);
                }
                
                return this.responder.Respond("common.done", this.CommandSource);
            }
            
            return this.responder.Respond("catwatcher.command.catwatcher.configure.not-changed", this.CommandSource);
        }

        [SubcommandInvocation("status")]
        [Help("", "Returns the current configuration of all the defined category watchers for this channel.")]
        protected IEnumerable<CommandResponse> StatusMode()
        {
            if (this.CommandSource == this.Client.Nickname)
            {
                return this.responder.Respond("catwatcher.command.catwatcher.status.not-via-pm", this.CommandSource);
            }

            var watchers = this.catWatcherConfig.GetWatchers().ToDictionary(x => x.Keyword);
            var watcherConfig = watchers
                .Select(x => this.catWatcherConfig.GetWatcherConfiguration(x.Key, this.CommandSource, true))
                .ToDictionary(x => x.Watcher);

            var enabled = this.responder.GetMessagePart("catwatcher.command.catwatcher.status.enabled", this.CommandSource);
            var disabled = this.responder.GetMessagePart("catwatcher.command.catwatcher.status.disabled", this.CommandSource);
            var nullStatusmsg = this.responder.GetMessagePart("catwatcher.command.catwatcher.status.target-all", this.CommandSource);
            var nullAnchor = this.responder.GetMessagePart("catwatcher.command.catwatcher.status.anchor-none", this.CommandSource);

            
            var responses = new List<CommandResponse>(watchers.Keys.Count);
            
            foreach (var key in watchers.Keys)
            {
                responses.AddRange(
                    this.responder.Respond(
                        "catwatcher.command.catwatcher.status.watcher-status",
                        this.CommandSource,
                        new object[]
                        {
                            watchers[key].Keyword,
                            watchers[key].Category,
                            watcherConfig[key].Enabled ? enabled : disabled,
                            watcherConfig[key].SleepTime,
                            watcherConfig[key].AlertForAdditions ? enabled : disabled,
                            watcherConfig[key].AlertForRemovals ? enabled : disabled,
                            watcherConfig[key].ShowLink ? enabled : disabled,
                            watcherConfig[key].ShowWaitTime ? enabled : disabled,
                            watcherConfig[key].MinWaitTime,
                            string.IsNullOrWhiteSpace(watcherConfig[key].StatusMsg) ? nullStatusmsg : watcherConfig[key].StatusMsg,
                            string.IsNullOrWhiteSpace(watcherConfig[key].Anchor) ? nullAnchor : watcherConfig[key].Anchor
                        })
                );
            }
                
            return responses;
        }
    }
}