namespace Helpmebot.ChannelServices.Services
{
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Configuration;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.Configuration;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model;

    public class JoinMonitorService : IJoinMonitorService
    {
        private readonly IIrcClient client;
        private readonly ILogger logger;
        private readonly ModuleConfiguration moduleConfiguration;
        private readonly BotConfiguration config;

        public JoinMonitorService(
            IIrcClient client,
            ILogger logger,
            ModuleConfiguration moduleConfiguration,
            BotConfiguration config)
        {
            this.client = client;
            this.logger = logger;
            this.moduleConfiguration = moduleConfiguration;
            this.config = config;
        }
        
        public void Start()
        {
            this.client.JoinReceivedEvent += this.IrcJoinReceived;
        }
        public void Stop()
        {
            this.client.JoinReceivedEvent -= this.IrcJoinReceived;
        }

        private void IrcJoinReceived(object sender, JoinEventArgs e)
        {
            var monitorConfigs  = this.moduleConfiguration.JoinMonitors?.Where(m => m.Channel == e.Channel).ToList();

            if (monitorConfigs == null || monitorConfigs.Count == 0)
            {
                return;
            }
            
            foreach (var monitorConfig in monitorConfigs)
            {
                if (!new IrcUserMask(monitorConfig.Mask, this.client).Matches(e.User).GetValueOrDefault(false))
                {
                    return;
                }

                var message = $"User {e.User.Nickname} joined channel {e.Channel}";
                
                this.client.SendMessage(monitorConfig.Notify, message);    
            }
        }
    }
}