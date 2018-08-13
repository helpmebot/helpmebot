namespace Helpmebot.Background
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Timers;
    using Castle.Core.Logging;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
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
            ICategoryWatcherHelperService helperService)
            : base(logger, configuration.UpdateFrequency * 1000, configuration.Enabled)
        {
            this.crossoverTimeout = configuration.CrossoverTimeout;
            this.ircClient = ircClient;
            this.helperService = helperService;
        }

        protected override void OnStart()
        {
            this.TimerOnElapsed(null, null);
        }

        public void ForceUpdate(string key, Channel destination)
        {
            this.Logger.DebugFormat("Force-update was triggered for {0} in {1}", key, destination.Name);
            
            // Locks!
            this.timerSemaphore.WaitOne();

            try
            {
                var watcher = this.helperService.WatchedCategories.FirstOrDefault(x => x.Keyword == key);
                if (watcher == null)
                {
                    throw new ArgumentOutOfRangeException("key");
                }

                this.Logger.DebugFormat("Found watcher {0} for {1}", key, watcher.Category);

                var channel = watcher.Channels.FirstOrDefault(x => x.Channel == destination);
                if (channel == null)
                {
                    this.Logger.DebugFormat("Faking channelconfig for {0}", destination);

                    channel = new CategoryWatcherChannel
                    {
                        AlertForAdditions = false,
                        AlertForRemovals = false,
                        Channel = destination,
                        MinWaitTime = 3600,
                        ShowWaitTime = true,
                        ShowLink = true,
                        SleepTime = 10000,
                        Watcher = watcher
                    };
                }

                this.helperService.UpdateCategoryItems(watcher);

                var message = this.helperService.ConstructDefaultMessage(
                    watcher,
                    channel,
                    watcher.CategoryItems.ToList(),
                    false,
                    true);

                if (message != null)
                {
                    this.ircClient.SendMessage(destination.Name, message);
                }
            }
            catch (WebException ex)
            {
                this.ircClient.SendMessage(destination.Name, "Could not retrieve category items due to Wikimedia API error");
                this.Logger.Warn("Error during API fetch", ex);
            }
            catch (Exception ex)
            {
                this.Logger.ErrorFormat(ex, "Error encountered updating catwatcher for {0}", key);
                throw;
            }
            finally
            {
                this.timerSemaphore.Release();
            }
        }

        protected override void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                if (!this.timerSemaphore.WaitOne(new TimeSpan(0, 0, 0, this.crossoverTimeout)))
                {
                    this.Logger.WarnFormat(
                        "Semaphore timeout ({0}s) reached on timer trigger. Perhaps we're trying to do too much?");
                    return;
                }

                foreach (var category in this.helperService.WatchedCategories.Where(x => x.Channels.Any()))
                {
                    var result = this.helperService.UpdateCategoryItems(category);
                    var additions = result.Item1;
                    var removals = result.Item2;

                    foreach (var categoryChannel in category.Channels)
                    {
                        if (categoryChannel.Channel.Silenced)
                        {
                            this.Logger.DebugFormat(
                                "Not reporting to {0}, bot is silenced",
                                categoryChannel.Channel.Name);
                            continue;
                        }

                        if (categoryChannel.AlertForRemovals && removals.Any())
                        {
                            var removalList = string.Join(
                                ", ",
                                removals.Select(x => string.Format("[[{0}]]", x.Title)));
                            this.ircClient.SendMessage(
                                categoryChannel.Channel.Name,
                                string.Format("Handled: {0}", removalList));
                        }

                        if (!this.alertTimeoutCache.ContainsKey(categoryChannel.Id))
                        {
                            this.alertTimeoutCache.Add(categoryChannel.Id, DateTime.MinValue);
                            this.Logger.DebugFormat(
                                "Adding timeout cache entry for {0}/{1}/{2}",
                                categoryChannel.Id,
                                categoryChannel.Channel.Name,
                                category.Keyword);
                        }

                        if (this.alertTimeoutCache[categoryChannel.Id] <= DateTime.Now)
                        {
                            this.Logger.DebugFormat(
                                "Timeout reached for {0}/{1}/{2}",
                                categoryChannel.Id,
                                categoryChannel.Channel.Name,
                                category.Keyword);

                            this.alertTimeoutCache[categoryChannel.Id] =
                                DateTime.Now.AddSeconds(categoryChannel.SleepTime);

                            var message = this.helperService.ConstructDefaultMessage(
                                category,
                                categoryChannel,
                                category.CategoryItems.ToList(),
                                false,
                                false);

                            if (message != null)
                            {
                                this.ircClient.SendMessage(categoryChannel.Channel.Name, message);
                            }
                        }
                        else
                        {
                            if (categoryChannel.AlertForAdditions && additions.Any())
                            {
                                var message = this.helperService.ConstructDefaultMessage(
                                    category,
                                    categoryChannel,
                                    additions,
                                    true,
                                    false);

                                if (message != null)
                                {
                                    this.ircClient.SendMessage(categoryChannel.Channel.Name, message);
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                this.Logger.Error("Error during fetch from API", ex);
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
        }
    }
}