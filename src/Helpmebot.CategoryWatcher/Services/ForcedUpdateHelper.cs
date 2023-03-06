namespace Helpmebot.CategoryWatcher.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using CoreServices.Services.Messages.Interfaces;
    using Interfaces;
    using Model;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Array = System.Array;

    public class ForcedUpdateHelper : IForcedUpdateHelper
    {
        private readonly ILogger logger;
        private readonly IWatcherConfigurationService watcherConfig;
        private readonly ICategoryWatcherHelperService helper;
        private readonly IWatcherConfigurationService watcherConfigurationService;
        private readonly IResponder responder;

        public ForcedUpdateHelper(
            ILogger logger,
            IWatcherConfigurationService watcherConfig,
            ICategoryWatcherHelperService helper,
            IWatcherConfigurationService watcherConfigurationService,
            IResponder responder)
        {
            this.logger = logger;
            this.watcherConfig = watcherConfig;
            this.helper = helper;
            this.watcherConfigurationService = watcherConfigurationService;
            this.responder = responder;
        }

        public IEnumerable<CommandResponse> DoForcedUpdate(string categoryKeyword, string channelName)
        {
            var config = this.watcherConfig.GetWatcherConfiguration(categoryKeyword, channelName, true);
            
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
                
                if (!validKeywords.Any())
                {
                    return this.responder.Respond(
                        "catwatcher.command.fetchall.none-configured",
                        channelName,
                        Array.Empty<object>());
                }
            }
            else
            {
                validKeywords = this.watcherConfigurationService.GetWatchersForChannel(channelName).ToList();

                if (!validKeywords.Any())
                {
                    return this.responder.Respond(
                        "catwatcher.command.fetchall.none-configured-in-channel",
                        channelName,
                        channelName);
                }
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