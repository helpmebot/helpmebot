namespace Helpmebot.AccountCreations.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Castle.Core.Logging;
    using Helpmebot.AccountCreations.Configuration;
    using Helpmebot.AccountCreations.Services.Interfaces;
    using Helpmebot.Configuration;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using ModuleConfiguration = Helpmebot.AccountCreations.Configuration.ModuleConfiguration;

    public class MqNotificationService : IMqNotificationService, IDisposable
    {
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly string userAgent;

        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;
        private readonly RabbitMqConfiguration configuration;

        public MqNotificationService(ModuleConfiguration configuration, ILogger logger, BotConfiguration botConfiguration, IIrcClient client)
        {
            this.configuration = configuration.MqConfiguration;
            this.logger = logger;
            this.client = client;
            this.userAgent = botConfiguration.UserAgent;
        }
        
        public void Start()
        {
            this.logger.Debug("Starting MQ connection...");

            if (!this.configuration.Enabled)
            {
                this.logger.Warn("RabbitMQ disabled, refusing to start.");
                return;
            }
            
            var factory = new ConnectionFactory
            {
                HostName = this.configuration.Hostname,
                Port = this.configuration.Port,
                VirtualHost = this.configuration.VirtualHost,
                UserName = this.configuration.Username,
                Password = this.configuration.Password,
                ClientProvidedName = this.userAgent
            };

            this.connection = factory.CreateConnection();
            this.channel = this.connection.CreateModel();

            var exchange = this.configuration.ObjectPrefix + ".x.notification";
            var dlExchange = this.configuration.ObjectPrefix + ".x.notification-dead";
            var queue = this.configuration.ObjectPrefix + ".q.notification";
            var dlQueue = this.configuration.ObjectPrefix + ".q.notification-dead";

            var exchangeConfig = new Dictionary<string, object> { { "alternate-exchange", dlExchange } };
            
            this.channel.ExchangeDeclare(dlExchange, "fanout", true);
            this.channel.ExchangeDeclare(exchange, "direct", true, false, exchangeConfig);
            this.channel.QueueDeclare(queue, true, false, false);
            this.channel.QueueDeclare(dlQueue, true, false, false);

            // bind the dead letter queue
            this.channel.QueueBind(dlQueue, dlExchange, string.Empty);
            
            this.consumer = new EventingBasicConsumer(this.channel);
            this.consumer.Received += this.ConsumerOnReceived;
            
            this.channel.BasicConsume(queue, true, this.consumer);
            this.logger.Debug("Connected.");
        }

        public void Stop()
        {
            if (!this.configuration.Enabled)
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
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            this.client.SendMessage(e.RoutingKey, message);
        }

        public void Dispose()
        {
            this.connection?.Dispose();
            this.channel?.Dispose();
        }
    }
}