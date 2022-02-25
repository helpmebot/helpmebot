namespace Helpmebot.AccountCreations.Services.Interfaces
{
    using System.Collections.Generic;

    public interface INotificationHelper
    {
        string SanitiseMessage(string text);

        void DeliverNotification(string text, List<string> targets);
    }
}