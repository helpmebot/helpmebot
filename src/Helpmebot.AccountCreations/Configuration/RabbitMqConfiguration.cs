namespace Helpmebot.AccountCreations.Configuration
{
    public class RabbitMqConfiguration
    {
        public RabbitMqConfiguration(bool enabled, string notificationQueue)
        {
            this.Enabled = enabled;
            this.NotificationQueue = notificationQueue;
        }
        
        public bool Enabled { get; }
        public string NotificationQueue { get; }
        public string Hostname { get; set; } = "localhost";
        public ushort Port { get; set; } = 5672;
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }
}