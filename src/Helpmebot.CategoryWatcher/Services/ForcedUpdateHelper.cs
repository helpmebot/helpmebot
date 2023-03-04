namespace Helpmebot.CategoryWatcher.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Interfaces;
    using Model;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;

    public class ForcedUpdateHelper : IForcedUpdateHelper
    {
        private readonly ILogger logger;
        private readonly IWatcherConfigurationService watcherConfig;
        private readonly ICategoryWatcherHelperService helper;
        private readonly IWatcherConfigurationService watcherConfigurationService;

        public ForcedUpdateHelper(
            ILogger logger,
            IWatcherConfigurationService watcherConfig,
            ICategoryWatcherHelperService helper,
            IWatcherConfigurationService watcherConfigurationService)
        {
            this.logger = logger;
            this.watcherConfig = watcherConfig;
            this.helper = helper;
            this.watcherConfigurationService = watcherConfigurationService;
        }

        public IEnumerable<CommandResponse> DoForcedUpdate(string categoryKeyword, string channelName)
        {
            var config = this.watcherConfig.GetWatcherConfiguration(categoryKeyword, channelName);

            if (config == null)
            {
                yield return new CommandResponse
                {
                    Message = "FIXME: not configured in this channel.",
                    Destination = CommandResponseDestination.PrivateMessage,
                    IgnoreRedirection = true
                };

                config = new CategoryWatcherChannel
                {
                    AlertForAdditions = false,
                    AlertForRemovals = false,
                    MinWaitTime = 0,
                    ShowLink = false,
                    ShowWaitTime = false,
                    SleepTime = 20 * 60
                };
            }

            var (allItems, added, removed) = this.helper.SyncCategoryItems(categoryKeyword);

            var message = this.helper.ConstructResultMessage(
                allItems,
                categoryKeyword,
                channelName,
                false,
                true,
                config.ShowLink,
                config.ShowWaitTime,
                config.MinWaitTime
            );

            yield return new CommandResponse { Message = message };
        }

        public IEnumerable<CommandResponse> BulkForcedUpdate(bool all, string channelName)
        {
            List<string> validKeywords;
            if (all)
            {
                validKeywords = this.watcherConfigurationService.GetValidWatcherKeys().ToList();
            }
            else
            {
                validKeywords = this.watcherConfigurationService.GetWatchersForChannel(channelName).ToList();
            }

            var responses = new List<CommandResponse>();
            foreach (var keyword in validKeywords)
            {
                responses.AddRange(this.DoForcedUpdate(keyword, channelName));
            }

            return responses;
        }
    }
}