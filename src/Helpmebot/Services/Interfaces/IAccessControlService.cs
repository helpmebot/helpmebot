namespace Helpmebot.Services.Interfaces
{
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public interface IAccessControlService : IFlagService
    {
        void CreateFlagGroup(string name, string flags, ISession session);
        void ModifyFlagGroup(string name, string flags, ISession session);
        void SetFlagGroup(string name, string flags, ISession session);
        void DeleteFlagGroup(string name, ISession session);

        void GrantFlagGroupGlobally(IUser user, FlagGroup group);
        void RevokeFlagGroupGlobally(IUser user, FlagGroup group);
    }
}