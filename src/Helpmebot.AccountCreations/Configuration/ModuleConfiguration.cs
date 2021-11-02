// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Helpmebot.AccountCreations.Configuration
{
    public class ModuleConfiguration
    {
        public string DeploymentPassword { get; set; }
        
        public NotificationReceiverConfiguration Notifications { get; set; }
        
        public RabbitMqConfiguration MqConfiguration { get; set; }
    }
}