namespace Helpmebot.CategoryWatcher.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Timers;
    using Castle.Core.Logging;
    using CoreServices.Services.Interfaces;
    using Helpmebot.Background;
    using Helpmebot.CategoryWatcher.Configuration;
    using Helpmebot.CategoryWatcher.Services.Interfaces;
    using Helpmebot.Model;
    using Stwalkerster.IrcClient.Interfaces;

    /// <summary>
    /// Manages the event loop for monitoring, and reporting triggers from the event loop to IRC.
    ///
    /// This class does not manage any of the watched categories, that should all be done via <see cref="ICategoryWatcherHelperService"/>
    /// </summary>
    public class CategoryWatcherBackgroundService : TimerBackgroundServiceBase, ICategoryWatcherBackgroundService
    {
        /// <summary>
        /// Timeout between runs
        /// </summary>
        private readonly int crossoverTimeout;

        private readonly IIrcClient ircClient;
        private readonly ICategoryWatcherHelperService helperService;
        private readonly IWatcherConfigurationService watcherConfig;
        private readonly IChannelManagementService channelManagementService;

        /// <summary>
        /// Stores the time the next alert should be sent.
        /// catchannel ID => next alert time 
        /// </summary>
        private readonly Dictionary<int, DateTime> alertTimeoutCache = new Dictionary<int, DateTime>();

        /// <remarks>
        /// This semaphore is used to prevent re-entrancy of the TimerOnElapsed method 
        /// </remarks>
        private readonly Semaphore timerSemaphore = new Semaphore(1, 1);

        public CategoryWatcherBackgroundService(
            ILogger logger,
            CategoryWatcherConfiguration configuration,
            IIrcClient ircClient,
            ICategoryWatcherHelperService helperService,
            IWatcherConfigurationService watcherConfig,
            IChannelManagementService channelManagementService)
            : base(logger, configuration.UpdateFrequency * 1000, configuration.Enabled)
        {
            this.crossoverTimeout = configuration.CrossoverTimeout;
            this.ircClient = ircClient;
            this.helperService = helperService;
            this.watcherConfig = watcherConfig;
            this.channelManagementService = channelManagementService;
        }

        public void Suspend()
        {
            this.timerSemaphore.WaitOne();
        }

        public void Resume()
        {
            this.timerSemaphore.Release();
        }
        
        protected override void OnStart()
        {
            this.Logger.DebugFormat("Starting CatWatcher");
            
            this.ircClient.WaitOnRegistration();
            Thread.Sleep(1000);

            this.TimerOnElapsed(null, null);
        }

        protected override void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            this.Logger.DebugFormat("CatWatcher timer elapsed");
            try
            {
                if (!this.timerSemaphore.WaitOne(new TimeSpan(0, 0, 0, this.crossoverTimeout)))
                {
                    this.Logger.WarnFormat(
                        "Semaphore timeout ({0}s) reached on timer trigger. Perhaps we're trying to do too much?",
                        this.crossoverTimeout);
                    return;
                }

                foreach (var watcher in this.watcherConfig.GetWatchers())
                {
                    var (allItems, added, removed) = this.helperService.SyncCategoryItems(watcher.Keyword);

                    var watcherChannels = this.watcherConfig.GetChannelsForWatcher(watcher.Keyword);

                    foreach (var channelName in watcherChannels)
                    {
                        if (!this.channelManagementService.IsEnabled(channelName))
                        {
                            // Config for watcher is in-place, but the channel itself is disabled.
                            continue;
                        }
                        
                        var config = this.watcherConfig.GetWatcherConfiguration(watcher.Keyword, channelName);
                        
                        if (config == null || !config.Enabled)
                        {
                            continue;
                        }

                        this.InitialiseTimer(config);

                        if (this.channelManagementService.IsSilenced(channelName))
                        {
                            this.Logger.InfoFormat("Not reporting to {0}, bot is silenced", channelName);
                            continue;
                        }

                        var responses = new List<string>();

                        responses.AddRange(this.AlertRemovals(removed, watcher.Keyword, config));
                        
                        // check if it's time to report everything
                        if (this.alertTimeoutCache[config.Id] <= DateTime.UtcNow)
                        {
                            if (!this.ircClient.Channels.ContainsKey(channelName))
                            {
                                this.Logger.WarnFormat("Timeout reached for {0}/{1}/{2} but not in channel!", config.Id, channelName, watcher.Keyword);
                                continue;
                            }
                            
                            this.Logger.DebugFormat("Timeout reached for {0}/{1}/{2}", config.Id, channelName, watcher.Keyword);
                            this.alertTimeoutCache[config.Id] = DateTime.UtcNow.AddSeconds(config.SleepTime);

                            responses.AddRange(this.AlertAllItems(allItems, watcher.Keyword, config));
                        }
                        else
                        {
                            responses.AddRange(this.AlertAdditions(added, watcher.Keyword, config));
                        }

                        responses.ForEach(x => this.ircClient.SendMessage(channelName, x));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error("Error encountered during catwatcher run", ex);
                // squelch
            }
            finally
            {
                this.timerSemaphore.Release();
            }
            this.Logger.DebugFormat("CatWatcher timer done");
        }

        private void InitialiseTimer(CategoryWatcherChannel categoryChannel)
        {
            if (!this.alertTimeoutCache.ContainsKey(categoryChannel.Id))
            {
                this.alertTimeoutCache.Add(categoryChannel.Id, DateTime.MinValue);
                this.Logger.DebugFormat("Adding timeout cache entry for {0}/{1}/{2}", categoryChannel.Id, categoryChannel.Channel, categoryChannel.Watcher);
            }
        }
        
        private IEnumerable<string> AlertAllItems(
            IList<CategoryWatcherItem> allItems,
            string watcherKeyword,
            CategoryWatcherChannel categoryChannel)
        {
            var message = this.helperService.ConstructResultMessage(
                allItems,
                watcherKeyword,
                categoryChannel.Channel,
                false,
                false,
                categoryChannel.ShowLink,
                categoryChannel.ShowWaitTime,
                categoryChannel.MinWaitTime
            );

            if (message != null)
            {
                yield return message;
            }
        }
        
        private IEnumerable<string> AlertAdditions(
            IList<CategoryWatcherItem> added,
            string watcherKeyword,
            CategoryWatcherChannel categoryChannel)
        {
            if (categoryChannel.AlertForAdditions && added.Any())
            {
                var message = this.helperService.ConstructResultMessage(
                    added,
                    watcherKeyword,
                    categoryChannel.Channel,
                    true,
                    false,
                    categoryChannel.ShowLink,
                    categoryChannel.ShowWaitTime,
                    categoryChannel.MinWaitTime
                );

                if (message != null)
                {
                    yield return message;
                }
            }
        }
        
        private IEnumerable<string> AlertRemovals(
            IList<CategoryWatcherItem> removed,
            string watcherKeyword,
            CategoryWatcherChannel categoryChannel)
        {
            if (categoryChannel.AlertForRemovals && removed.Any())
            {
                yield return this.helperService.ConstructRemovalMessage(removed, watcherKeyword, categoryChannel.Channel);
            }
        }
    }
}