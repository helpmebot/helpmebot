namespace Helpmebot.Legacy.Transitional
{
    using Helpmebot.Legacy.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public interface ILegacyAccessService
    {
        bool IsAllowed(LegacyUserRights required, IUser user);
        LegacyUserRights GetLegacyUserRights(IUser user);
    }
}