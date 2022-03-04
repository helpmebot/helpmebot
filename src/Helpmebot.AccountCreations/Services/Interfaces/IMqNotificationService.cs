namespace Helpmebot.AccountCreations.Services.Interfaces
{
    using Castle.Core;

    public interface IMqNotificationService : IStartable
    {
        bool Active { get; }
        
        void Bind(string ircChannel);
        void Unbind(string ircChannel);
    }
}