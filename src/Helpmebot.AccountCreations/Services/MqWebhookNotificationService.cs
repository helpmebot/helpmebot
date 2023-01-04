namespace Helpmebot.AccountCreations.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;
    using Castle.Core.Logging;
    using CoreServices.Services.Interfaces;
    using Helpmebot.AccountCreations.Configuration;
    using Helpmebot.AccountCreations.Services.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Background;
    using Model.Terraform;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using ModuleConfiguration = Helpmebot.AccountCreations.Configuration.ModuleConfiguration;

    public class MqWebhookNotificationService : IMqWebhookNotificationService
    {
        private readonly RabbitMqConfiguration mqConfig;
        private readonly IMqService mqService;
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly IUrlShorteningService urlShorteningService;
        private readonly NotificationReceiverConfiguration notificationConfig;
        private IModel channel;
        private EventingBasicConsumer consumer;
        private string exchange;
        private string queue;

        public bool Active { get; private set; }

        public MqWebhookNotificationService(
            RabbitMqConfiguration mqConfig,
            IMqService mqService,
            ModuleConfiguration configuration,
            ILogger logger,
            IIrcClient client,
            IUrlShorteningService urlShorteningService)
        {
            this.notificationConfig = configuration.Notifications;
            this.mqConfig = mqConfig;
            this.mqService = mqService;
            this.logger = logger;
            this.client = client;
            this.urlShorteningService = urlShorteningService;
        }

        public void Start()
        {
            this.logger.Debug("Starting MQ webhook notification service...");

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

            this.exchange = this.mqConfig.ObjectPrefix + ".x.webhook";
            var dlExchange = this.mqConfig.ObjectPrefix + ".x.notification.deadletter";
            this.queue = this.mqConfig.ObjectPrefix + ".q.webhook";

            var exchangeConfig = new Dictionary<string, object> { { "alternate-exchange", dlExchange } };

            this.channel.ExchangeDeclare(this.exchange, "direct", true, false, exchangeConfig);
            this.channel.QueueDeclare(this.queue, true, false, false);

            this.consumer = new EventingBasicConsumer(this.channel);
            this.consumer.Received += this.ConsumerOnReceived;

            this.channel.BasicConsume(this.queue, true, this.consumer);
            this.logger.Debug("Initialised MQ webhook notifications.");
        }

        public void Stop()
        {
            if (!this.Active)
            {
                return;
            }

            this.consumer.Received -= this.ConsumerOnReceived;
            this.consumer = null;
            this.logger.Debug("Stopped MQ webhook notifications.");
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
            try
            {
                var source = (string)e.BasicProperties.Headers["source"];
                this.logger.InfoFormat(
                    "Handling message for {0} from source <{1}>",
                    e.RoutingKey,
                    source);

                var rawPayload = Encoding.UTF8.GetString(e.Body.ToArray());
                this.logger.Debug(rawPayload);

                IEnumerable<string> messages = new List<string>();
                switch (source)
                {
                    case "terraformcloud":
                        messages = this.HandleTerraform(rawPayload);
                        break;
                    case "github":
                        messages = this.HandleGithub(
                            rawPayload,
                            e.BasicProperties.Headers["x-github-event"].ToString());
                        break;
                }

                foreach (var s in messages)
                {
                    this.client.SendMessage(e.RoutingKey, s);
                }
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Something went wrong processing inbound webhook notification");
            }
        }

        private IEnumerable<string> HandleGithub(string rawPayload, string githubEvent)
        {
            yield break;
        }

        private IEnumerable<string> HandleTerraform(string rawPayload)
        {
            const char BoldMarker = (char)0x02;
            const char ColourMarker = (char)0x03;
            const char ClearMarker = (char)0x0f;
            const string Yellow = "08";
            const string Purple = "06";
            const string Red = "04";
            const string Green = "03";
            const string Orange = "07";
            const string Blue = "12";

            var payload = JsonSerializer.Deserialize<RunPayload>(rawPayload);

            var prefix =
                $"[{BoldMarker}{ColourMarker}{Purple}Terraform{ClearMarker}][{ColourMarker}{Yellow}{payload.OrganizationName}/{payload.WorkspaceName}{ClearMarker}] ";

            var shorturl = this.urlShorteningService.Shorten(payload.RunUrl);
            
            foreach (var notification in payload.Notifications)
            {
                var colour = string.Empty;
                switch (notification.Trigger)
                {
                    case "run:created":
                        break;
                    case "run:planning":
                    case "run:applying":
                        colour = ColourMarker + Blue;
                        break;
                    case "run:needs_attention":
                        colour = ColourMarker + Orange;
                        break;
                    case "run:completed":
                        colour = ColourMarker + Green;
                        break;
                    case "run:errored":
                        colour = ColourMarker + Red;
                        break;
                }

                var authorship = string.Empty;
                if (notification.RunUpdatedBy != null)
                {
                    authorship = " by " + notification.RunUpdatedBy;
                }
                
                yield return prefix + $"{BoldMarker}{colour}{payload.Notifications[0].Message}{ClearMarker}{authorship}: {payload.RunMessage} <{shorturl}>";
            }
        }
    }
}