namespace Helpmebot.AccountCreations.Services
{
    using System.Text;
    using Castle.Core.Logging;
    using Helpmebot.AccountCreations.Configuration;
    using Helpmebot.AccountCreations.Services.Interfaces;
    using Helpmebot.Configuration;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using ModuleConfiguration = Helpmebot.AccountCreations.Configuration.ModuleConfiguration;

    public class MqNotificationService : IMqNotificationService
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
            this.consumer = new EventingBasicConsumer(this.channel);
            this.consumer.Received += this.ConsumerOnReceived;
            
            this.channel.BasicConsume(this.configuration.NotificationQueue, true, this.consumer);
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
            this.client.SendMessage("##stwalkerster-development", message);
        }
    }
}