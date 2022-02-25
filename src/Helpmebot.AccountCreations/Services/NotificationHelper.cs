namespace Helpmebot.AccountCreations.Services
{
    using System.Collections.Generic;
    using Helpmebot.AccountCreations.Services.Interfaces;
    using Prometheus;
    using Stwalkerster.IrcClient.Interfaces;

    public class NotificationHelper : INotificationHelper
    {
        private readonly IIrcClient client;

        private static readonly Counter NotificationsSent = Metrics.CreateCounter(
            "helpmebot_notifications_total",
            "Number of notifications sent",
            new CounterConfiguration
            {
                LabelNames = new[] {"channel", "origin"}
            });

        public NotificationHelper(IIrcClient client)
        {
            this.client = client;
        }

        public string SanitiseMessage(string text)
        {
            return text.Replace("\r", "").Replace("\n", "");
        }

        public void DeliverNotification(string text, List<string> targets)
        {
            foreach (var x in targets)
            {
                this.client.SendMessage(x, this.SanitiseMessage(text));
                NotificationsSent.WithLabels(x).Inc();
            }
        }
    }
}