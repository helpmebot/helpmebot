namespace Helpmebot.Services.Interfaces
{
    using Castle.Core;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public interface ICrossChannelService : IStartable
    {
        void Configure(Channel frontend, Channel backend, ISession localSession);
        void Deconfigure(Channel backend, ISession localSession);
        void NotificationStatus(Channel backend, bool status, ISession localSession);
        void NotificationMessage(Channel backend, string message, ISession localSession);
        void NotificationKeyword(Channel backend, string keyword, ISession localSession);
        void Notify(Channel frontend, string message, ISession localSession, IIrcClient client, IUser user);
    }
}