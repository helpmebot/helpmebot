namespace Helpmebot.Services.Interfaces
{
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public interface IAccessControlService : IFlagService
    {
        bool CreateFlagGroup(string name, string flags, ISession session);
        bool ModifyFlagGroup(string name, string flags, ISession session);
        bool SetFlagGroup(string name, string flags, ISession session);
        bool DeleteFlagGroup(string name, ISession session);

        bool GrantFlagGroupGlobally(IUser user, FlagGroup group);
        bool RevokeFlagGroupGlobally(IUser user, FlagGroup group);
    }
}