namespace Helpmebot.CoreServices.Services.Interfaces
{
    using Helpmebot.Model;
    using NHibernate;

    public interface IAccessControlManagementService
    {
        void CreateFlagGroup(string name, string flags, ISession session);
        void ModifyFlagGroup(string name, string flags, ISession session);
        void SetFlagGroup(string name, string flags, ISession session);
        void DeleteFlagGroup(string name, ISession session);

        void GrantFlagGroupGlobally(User user, FlagGroup group, ISession session);
        void RevokeFlagGroupGlobally(User user, FlagGroup group, ISession session);

        /// <summary>
        /// Either creates or retrieves an existing entry for a specified user mask
        /// </summary>
        User GetUserObject(string ircMask, ISession session);
    }
}