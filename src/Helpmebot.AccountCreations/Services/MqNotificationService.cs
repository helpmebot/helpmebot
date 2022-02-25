namespace Helpmebot.AccountCreations.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Castle.Core.Logging;
    using Helpmebot.AccountCreations.Configuration;
    using Helpmebot.AccountCreations.Services.Interfaces;
    using Helpmebot.Configuration;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using ModuleConfiguration = Helpmebot.AccountCreations.Configuration.ModuleConfiguration;

    public class MqNotificationService : IMqNotificationService, IDisposable
    {
        private readonly ILogger logger;
        private readonly string userAgent;

        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;
        private readonly RabbitMqConfiguration mqConfig;
        private readonly NotificationReceiverConfiguration notificationConfig;
        private readonly INotificationHelper helper;

        public MqNotificationService(
            ModuleConfiguration configuration, 
            ILogger logger,
            BotConfiguration botConfiguration,
            INotificationHelper helper)
        {
            this.mqConfig = configuration.MqConfiguration;
            this.notificationConfig = configuration.Notifications;
            this.logger = logger;
            this.helper = helper;
            this.userAgent = botConfiguration.UserAgent;
        }

        public void Start()
        {
            this.logger.Debug("Starting MQ connection...");

            if (!this.mqConfig.Enabled)
            {
                this.logger.Warn("RabbitMQ disabled, refusing to start.");
                return;
            }
            
            var factory = new ConnectionFactory
            {
                HostName = this.mqConfig.Hostname,
                Port = this.mqConfig.Port,
                VirtualHost = this.mqConfig.VirtualHost,
                UserName = this.mqConfig.Username,
                Password = this.mqConfig.Password,
                ClientProvidedName = this.userAgent
            };

            this.connection = factory.CreateConnection();
            this.channel = this.connection.CreateModel();

            var exchange = this.mqConfig.ObjectPrefix + ".x.notification";
            var dlExchange = this.mqConfig.ObjectPrefix + ".x.notification.deadletter";
            var queue = this.mqConfig.ObjectPrefix + ".q.notification";
            var dlQueue = this.mqConfig.ObjectPrefix + ".q.notification.deadletter";

            var exchangeConfig = new Dictionary<string, object> { { "alternate-exchange", dlExchange } };
            
            this.channel.ExchangeDeclare(dlExchange, "fanout", true);
            this.channel.ExchangeDeclare(exchange, "direct", true, false, exchangeConfig);
            this.channel.QueueDeclare(queue, true, false, false);
            this.channel.QueueDeclare(dlQueue, true, false, false);

            // bind the dead letter queue
            this.channel.QueueBind(dlQueue, dlExchange, string.Empty);
            
            // bind any declared targets
            foreach (var target in this.notificationConfig.NotificationTargets)
            {
                this.channel.QueueBind(queue, exchange, target.Key);
            }
            
            this.consumer = new EventingBasicConsumer(this.channel);
            this.consumer.Received += this.ConsumerOnReceived;
            
            this.channel.BasicConsume(queue, true, this.consumer);
            this.logger.Debug("Connected.");
        }

        public void Stop()
        {
            if (!this.mqConfig.Enabled)
            {
                return;
            }

            this.consumer.Received -= this.ConsumerOnReceived;
            this.logger.Debug("Stopped MQ connection.");
            this.channel.Close();
            this.connection.Close();
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

        public void Dispose()
        {
            this.connection?.Dispose();
            this.channel?.Dispose();
        }
    }
}