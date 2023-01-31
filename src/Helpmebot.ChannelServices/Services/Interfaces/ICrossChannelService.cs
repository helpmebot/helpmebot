namespace Helpmebot.ChannelServices.Services.Interfaces
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
        void SetNotificationStatus(Channel backend, bool status, ISession localSession);
        void SetNotificationMessage(Channel backend, string message, ISession localSession);
        void SetNotificationKeyword(Channel backend, string keyword, ISession localSession);
        bool GetNotificationStatus(Channel backend, ISession localSession);
        void Notify(Channel frontend, string message, ISession localSession, IIrcClient client, IUser user);
    }
}