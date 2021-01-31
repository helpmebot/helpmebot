namespace Helpmebot.CoreServices.Services.Interfaces
{
    using NHibernate;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public interface IHelpSyncService
    {
        void DoSync(IUser forUser, ISession databaseSession);
    }
}