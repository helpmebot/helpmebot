namespace Helpmebot.CategoryWatcher.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Attributes;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using CoreServices.Services.Interfaces;
    using Helpmebot.CategoryWatcher.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("fetchall")]
    [CommandFlag(Flags.Protected)]
    [HelpCategory("CatWatcher")]
    [HelpSummary("Returns the current state of all category watchers configured in the current channel.")]
    public class FetchAllCommand : CommandBase
    {
        private readonly ICategoryWatcherBackgroundService categoryWatcherService;
        private readonly IResponder responder;
        private readonly IWatcherConfigurationService watcherConfigurationService;
        private readonly IChannelManagementService channelManagementService;

        public FetchAllCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ICategoryWatcherBackgroundService categoryWatcherService,
            IResponder responder,
            IWatcherConfigurationService watcherConfigurationService,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.categoryWatcherService = categoryWatcherService;
            this.responder = responder;
            this.watcherConfigurationService = watcherConfigurationService;
            this.channelManagementService = channelManagementService;
        }

        [CommandParameter(
            "all",
            "Return the current state of all category watchers configured on the bot.",
            "all",
            typeof(bool))]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var allKeywords = this.Parameters.GetParameter("all", false);

            if (!this.channelManagementService.IsEnabled(this.CommandSource) && !allKeywords)
            {
                return this.responder.Respond("catwatcher.command.fetchall.must-run-in-channel", this.CommandSource);
            }

            List<string> validKeywords;
            if (allKeywords)
            {
                validKeywords = this.watcherConfigurationService.GetValidWatcherKeys().ToList();
            }
            else
            {
                validKeywords = this.watcherConfigurationService.GetWatchersForChannel(this.CommandSource).ToList();
            }

            foreach (var flag in validKeywords)
            {
                this.categoryWatcherService.ForceUpdate(flag, this.CommandSource);
            }

            return Array.Empty<CommandResponse>();
        }
    }
}