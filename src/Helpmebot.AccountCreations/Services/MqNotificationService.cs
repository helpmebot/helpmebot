namespace Helpmebot.AccountCreations.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Castle.Core.Logging;
    using Helpmebot.AccountCreations.Configuration;
    using Helpmebot.AccountCreations.Services.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Background;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using ModuleConfiguration = Helpmebot.AccountCreations.Configuration.ModuleConfiguration;

    public class MqNotificationService : IMqNotificationService
    {
        private readonly RabbitMqConfiguration mqConfig;
        private readonly IMqService mqService;
        private readonly ILogger logger;
        private readonly NotificationReceiverConfiguration notificationConfig;
        private readonly INotificationHelper helper;
        private IModel channel;
        private EventingBasicConsumer consumer;
        private string exchange;
        private string queue;
        
        public bool Active { get; private set; }

        public MqNotificationService(
            RabbitMqConfiguration mqConfig,
            IMqService mqService,
            ModuleConfiguration configuration,
            ILogger logger,
            INotificationHelper helper)
        {
            this.notificationConfig = configuration.Notifications;
            this.mqConfig = mqConfig;
            this.mqService = mqService;
            this.logger = logger;
            this.helper = helper;
        }

        public void Start()
        {
            this.logger.Debug("Starting MQ notification service...");

            this.channel = this.mqService.CreateChannel();

            if (!this.notificationConfig.MqEnabled)
            {
                this.logger.Warn("Notifications are disabled.");
                return;
            }

            if (this.channel == null || this.channel.IsClosed)
            {
                this.logger.Warn("Unable to acquire channel.");
                return;
            }

            this.Active = true;

            this.exchange = this.mqConfig.ObjectPrefix + ".x.notification";
            var dlExchange = this.mqConfig.ObjectPrefix + ".x.notification.deadletter";
            this.queue = this.mqConfig.ObjectPrefix + ".q.notification";
            var dlQueue = this.mqConfig.ObjectPrefix + ".q.notification.deadletter";

            var exchangeConfig = new Dictionary<string, object> { { "alternate-exchange", dlExchange } };

            this.channel.ExchangeDeclare(dlExchange, "fanout", true);
            this.channel.ExchangeDeclare(this.exchange, "direct", true, false, exchangeConfig);
            this.channel.QueueDeclare(this.queue, true, false, false);
            this.channel.QueueDeclare(dlQueue, true, false, false);

            // bind the dead letter queue
            this.channel.QueueBind(dlQueue, dlExchange, string.Empty);

            // bind any declared targets
            foreach (var target in this.notificationConfig.NotificationTargets)
            {
                this.channel.QueueBind(this.queue, this.exchange, target.Key);
            }

            this.consumer = new EventingBasicConsumer(this.channel);
            this.consumer.Received += this.ConsumerOnReceived;

            this.channel.BasicConsume(this.queue, true, this.consumer);
            this.logger.Debug("Initialised MQ notifications.");
        }

        public void Stop()
        {
            if (!this.Active)
            {
                return;
            }

            this.consumer.Received -= this.ConsumerOnReceived;
            this.consumer = null;
            this.logger.Debug("Stopped MQ notifications.");
            this.mqService.ReturnChannel(this.channel);
            this.channel = null;
        }

        public void Bind(string ircChannel)
        {
            if (this.Active && this.channel != null)
            {
                this.channel.QueueBind(this.queue, this.exchange, ircChannel);
            }
        }

        public void Unbind(string ircChannel)
        {
            if (this.Active && this.channel != null)
            {
                this.channel.QueueUnbind(this.queue, this.exchange, ircChannel);
            }
        }
        
        
        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            if (!e.BasicProperties.IsAppIdPresent())
            {
                this.logger.WarnFormat("Refusing to deliver message missing appid");
                return;
            }
            
            if (!e.BasicProperties.IsUserIdPresent())
            {
                this.logger.WarnFormat("Refusing to deliver message missing userid");
                return;
            }
            
            this.logger.InfoFormat(
                "Handling message for {0} from user <{1}> app <{2}>",
                e.RoutingKey,
                e.BasicProperties.UserId,
                e.BasicProperties.AppId);
            
            var destinations = new List<string> { e.RoutingKey };
            if (this.notificationConfig.NotificationTargets.ContainsKey(e.RoutingKey))
            {
                destinations = this.notificationConfig.NotificationTargets[e.RoutingKey].ToList();
            }

            var appId = "amqp:" + e.BasicProperties.AppId;
            
            this.helper.DeliverNotification(Encoding.UTF8.GetString(e.Body.ToArray()), destinations, appId);
        }
    }
}