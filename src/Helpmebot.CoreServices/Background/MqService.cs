namespace Helpmebot.CoreServices.Background
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Commands;
    using RabbitMQ.Client;

    public class MqService : IMqService
    {
        private readonly RabbitMqConfiguration mqConfig;
        private readonly ILogger logger;
        private readonly BotConfiguration botConfiguration;
        private IConnection connection;

        private readonly List<IModel> channels = new List<IModel>();

        public MqService(RabbitMqConfiguration mqConfig, ILogger logger, BotConfiguration botConfiguration)
        {
            this.mqConfig = mqConfig;
            this.logger = logger;
            this.botConfiguration = botConfiguration;
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
                ClientProvidedName = this.botConfiguration.UserAgent,
                ClientProperties = new Dictionary<string, object>
                {
                    {"product", Encoding.UTF8.GetBytes("Helpmebot")},
                    {"version", Encoding.UTF8.GetBytes(VersionCommand.BotVersion)},
                    {"platform", Encoding.UTF8.GetBytes(RuntimeInformation.FrameworkDescription)},
                    {"os", Encoding.UTF8.GetBytes(Environment.OSVersion.ToString())}
                },
            };

            this.connection = factory.CreateConnection();
            this.logger.Debug("Connected.");
        }

        public void Stop()
        {
            if (!this.mqConfig.Enabled)
            {
                return;
            }

            foreach (var channel in this.channels)
            {
                channel.Close();
            }

            this.connection.Close();
        }

        public IModel CreateChannel()
        {
            if (!this.mqConfig.Enabled || !this.connection.IsOpen)
            {
                return null;
            }

            var model = this.connection.CreateModel();
            this.channels.Add(model);
            return model;
        }

        public void ReturnChannel(IModel channel)
        {
            if (channel == null)
            {
                return;
            }

            if (this.channels.Contains(channel))
            {
                this.channels.Remove(channel);
                if (channel.IsOpen)
                {
                    channel.Close();
                }
            }
        }
    }
}