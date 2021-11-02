namespace Helpmebot.CoreServices.Services.Interfaces
{
    using Stwalkerster.IrcClient.Model.Interfaces;

    public interface IHelpSyncService
    {
        void DoSync(IUser forUser);
    }
}