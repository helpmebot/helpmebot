namespace Helpmebot.Configuration
{
    using System.Collections.Generic;

    public class NotificationReceiverConfiguration
    {
        public bool Enable { get; }

        public IDictionary<string, IList<string>> NotificationTargets { get; }

        public int PollingInterval { get; }

        public NotificationReceiverConfiguration(
            bool enable,
            IDictionary<string, IList<string>> notificationTargets,
            int pollingInterval)
        {
            this.PollingInterval = pollingInterval;
            this.Enable = enable;
            this.NotificationTargets = notificationTargets;
        }
    }
}