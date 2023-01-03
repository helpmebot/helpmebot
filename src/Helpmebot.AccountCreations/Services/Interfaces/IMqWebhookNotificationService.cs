namespace Helpmebot.AccountCreations.Services.Interfaces
{
    using Castle.Core;

    public interface IMqWebhookNotificationService : IStartable
    {
        bool Active { get; }
        
        void Bind(string ircChannel);
        void Unbind(string ircChannel);
    }
}