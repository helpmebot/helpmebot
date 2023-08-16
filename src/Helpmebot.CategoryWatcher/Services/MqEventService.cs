namespace Helpmebot.CategoryWatcher.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.CategoryWatcher.Configuration;
    using Helpmebot.CategoryWatcher.EventStreams.Model;
    using Helpmebot.CategoryWatcher.Services.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Background;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using Newtonsoft.Json;
    using Prometheus;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using Stwalkerster.IrcClient.Interfaces;

    public class MqEventService : IMqEventService
    {
        private readonly RabbitMqConfiguration mqConfig;
        private readonly IMqService mqService;
        private readonly ILogger logger;
        private readonly ICategoryWatcherHelperService catwatcherHelperService;
        private readonly IWatcherConfigurationService catwatcherConfig;
        private readonly IItemPersistenceService itemPersistenceService;
        private readonly IChannelManagementService channelManagementService;
        private readonly IIrcClient client;
        private readonly CategoryWatcherConfiguration moduleConfig;
        private IModel channel;
        private string exchange;

        private string queue;
        private EventingBasicConsumer consumer;
        private readonly Regex commentRegex;
        private List<string> categoryFilter;

        private static readonly Counter EventsReceived = Metrics.CreateCounter(
            "helpmebot_catwatcher_es_events_received_total",
            "Total number of catwatcher events received via MQ",
            new CounterConfiguration());        
        
        private static readonly Counter EventsAccepted = Metrics.CreateCounter(
            "helpmebot_catwatcher_es_events_accepted_total",
            "Total number of catwatcher events received and accepted via MQ",
            new CounterConfiguration());
        
        private static readonly Gauge EventsWaiting = Metrics.CreateGauge(
            "helpmebot_catwatcher_es_events_waiting",
            "Number of events waiting to be processed.",
            new GaugeConfiguration());

        private static readonly Counter LogEventsProcessed = Metrics.CreateCounter(
            "helpmebot_catwatcher_es_log_events_total",
            "Number of log events processed as far as doing a database lookup",
            new CounterConfiguration());
        
        public bool Active { get; private set; }

        public MqEventService(
            RabbitMqConfiguration mqConfig,
            IMqService mqService,
            ILogger logger,
            ICategoryWatcherHelperService catwatcherHelperService,
            IWatcherConfigurationService catwatcherConfig,
            IItemPersistenceService itemPersistenceService,
            IChannelManagementService channelManagementService,
            IIrcClient client,
            CategoryWatcherConfiguration moduleConfig
            )
        {
            this.mqConfig = mqConfig;
            this.mqService = mqService;
            this.logger = logger;
            this.catwatcherHelperService = catwatcherHelperService;
            this.catwatcherConfig = catwatcherConfig;
            this.itemPersistenceService = itemPersistenceService;
            this.channelManagementService = channelManagementService;
            this.client = client;
            this.moduleConfig = moduleConfig;
            this.commentRegex = new Regex(@"^\[\[:(.*)\]\] (added|removed) (?:to|from) category");
            this.categoryFilter = this.catwatcherConfig.GetWatchers().Select(x => x.Category).ToList();
        }

        public void Start()
        {
            if (!this.moduleConfig.UseMq)
            {
                this.logger.Warn("MQ CatWatcher service is disabled.");
                return;
            }
            
            this.logger.Debug("Starting MQ event service...");
            this.channel = this.mqService.CreateChannel();

            if (this.channel == null || this.channel.IsClosed)
            {
                this.logger.Warn("Unable to acquire channel.");
                return;
            }

            this.Active = true;

            this.exchange = this.mqConfig.ObjectPrefix + ".x.catwatcher-events";
            this.queue = this.mqConfig.ObjectPrefix + ".q.catwatcher-events";
            
            var exchangeConfig = new Dictionary<string, object> { { "internal", true } };
            var queueConfig = new Dictionary<string, object> { { "x-message-ttl", 900000 }, { "x-max-length", 1000 } };

            this.channel.ExchangeDeclare(this.exchange, "fanout", true, false, exchangeConfig);
            this.channel.QueueDeclare(this.queue, true, false, false, queueConfig);

            var queueBindingConfig = new Dictionary<string, object>() ;
            this.channel.QueueBind(this.queue, this.exchange, string.Empty, queueBindingConfig);
            
            this.consumer = new EventingBasicConsumer(this.channel);
            this.consumer.Received += this.ConsumerOnReceived;

            this.channel.BasicConsume(this.queue, false, this.consumer);
            this.logger.Debug("Initialised MQ notifications.");

            this.catwatcherConfig.WatcherConfigurationChanged += this.OnWatcherConfigChanged;
        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            EventsWaiting.Set(this.channel.MessageCount(this.queue));
            
            var content = Encoding.UTF8.GetString(e.Body.ToArray());
            var recentChange = JsonConvert.DeserializeObject<RecentChange>(content);
            
            EventsReceived.Inc();
            
            if (recentChange.Type != "categorize" && recentChange.Type != "log")
            {
                // not an event we care about
                this.channel.BasicAck(e.DeliveryTag, false);
                return;
            }
            
            switch (recentChange.Type)
            {
                case "categorize":
                    this.HandleCategorizeEvent(e, recentChange);
                    break;
                case "log":
                    this.HandleLogEvent(e, recentChange);
                    break;
                default:
                    // Not an event we care about
                    this.channel.BasicAck(e.DeliveryTag, false);
                    break;
            }
        }

        private void HandleLogEvent(BasicDeliverEventArgs e, RecentChange recentChange)
        {
            if (recentChange.LogType != "delete")
            {
                // not an event we care about
                this.channel.BasicAck(e.DeliveryTag, false);
                return;
            }

            var allowableDeleteActions = new[] { "delete", "delete_redir" };
            
            if (!allowableDeleteActions.Contains(recentChange.LogAction))
            {
                // not an event we care about
                this.channel.BasicAck(e.DeliveryTag, false);
                return;
            }

            LogEventsProcessed.Inc();
            
            var watchers = this.itemPersistenceService.PageIsTracked(recentChange.Title)
                .Select(x => x.Watcher)
                .ToList();

            this.logger.DebugFormat(
                "Found EventStreams log delete event for page {0}; affected {1} watchers",
                recentChange.Title,
                watchers.Count);
            
            foreach (var w in watchers)
            {
                this.SyncEvent(w, recentChange.Title, false);
            }
            
            this.channel.BasicAck(e.DeliveryTag, false);
        }

        private void HandleCategorizeEvent(BasicDeliverEventArgs e, RecentChange recentChange)
        {
            if (!this.categoryFilter.Contains(recentChange.Title))
            {
                // not an event we care about
                this.channel.BasicAck(e.DeliveryTag, false);
                return;
            }

            var categoryWatcher = this.catwatcherConfig.GetWatchers()
                .First(x => x.Category == recentChange.Title);

            if (categoryWatcher.LastSyncTime > recentChange.Meta.DateTime)
            {
                // event too old
                this.channel.BasicAck(e.DeliveryTag, false);
                return;
            }

            var match = this.commentRegex.Match(recentChange.Comment);
            var pageTitle = match.Groups[1].Value;
            var added = match.Groups[2].Value == "added";

            this.logger.DebugFormat(
                "Found EventStreams categorize event for category {0}, page {1}; added={2}",
                recentChange.Title,
                pageTitle,
                added);

            this.channel.BasicAck(e.DeliveryTag, false);
            EventsAccepted.Inc();

            this.SyncEvent(categoryWatcher.Keyword, pageTitle, added);
        }

        private void SyncEvent(string keyword, string pageTitle, bool added)
        {
            var channels = this.catwatcherConfig.GetChannelsForWatcher(keyword);

            var pageTitles = new[] { pageTitle };

            IList<CategoryWatcherItem> itemList;
            if (added)
            {
                itemList = this.itemPersistenceService.AddNewItems(keyword, pageTitles);
            }
            else
            {
                var currentItems = this.itemPersistenceService.GetItems(keyword).ToList();
                itemList = pageTitles.Select(x => currentItems.FirstOrDefault(y => y.Title == x)).ToList();
                
                this.itemPersistenceService.RemoveDeletedItems(keyword, pageTitles);
            }

            ((CategoryWatcherHelperService)this.catwatcherHelperService).TouchCategoryItemsMetric(keyword, added);
            
            this.BroadcastEventToIrc(keyword, added, channels, itemList);
        }

        private void BroadcastEventToIrc(string keyword, bool added, IEnumerable<string> channels, IList<CategoryWatcherItem> itemList)
        {
            foreach (var channelName in channels)
            {
                if (!this.channelManagementService.IsEnabled(channelName))
                {
                    // Config for watcher is in-place, but the channel itself is disabled.
                    continue;
                }

                if (this.channelManagementService.IsSilenced(channelName))
                {
                    continue;
                }

                var config = this.catwatcherConfig.GetWatcherConfiguration(keyword, channelName);

                if (config == null || !config.Enabled)
                {
                    continue;
                }

                string message = null;
                if (config.AlertForAdditions && added)
                {
                    message = this.catwatcherHelperService.ConstructResultMessage(
                        itemList,
                        keyword,
                        config.Channel,
                        true,
                        false,
                        config.ShowLink,
                        config.ShowWaitTime,
                        config.MinWaitTime
                    );
                }

                if (config.AlertForRemovals && !added)
                {
                    message = this.catwatcherHelperService.ConstructRemovalMessage(
                        itemList,
                        keyword,
                        config.Channel);
                }

                if (message != null)
                {
                    this.client.SendMessage(config.Channel, message);
                }
            }
        }

        public void Stop()
        {
            if (!this.Active)
            {
                return;
            }
            
            this.catwatcherConfig.WatcherConfigurationChanged -= this.OnWatcherConfigChanged;
            this.consumer.Received -= this.ConsumerOnReceived;
            this.consumer = null;
            this.logger.Debug("Stopped MQ notifications.");
            this.mqService.ReturnChannel(this.channel);
            this.channel = null;
        }

        private void OnWatcherConfigChanged(object sender, EventArgs e)
        {
            this.categoryFilter = this.catwatcherConfig.GetWatchers().Select(x => x.Category).ToList();
        }
    }
}