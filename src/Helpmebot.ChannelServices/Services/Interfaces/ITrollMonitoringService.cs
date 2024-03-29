namespace Helpmebot.ChannelServices.Services.Interfaces
{
    using Castle.Core;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public interface ITrollMonitoringService : IStartable
    {
        void EnactBan(IUser enactingUser);
        void ForceAddTracking(IUser user, object sender);
        void SetScore(IUser user, int score, bool checkFirstMessage);
        IUser SetScore(string nickname, int score, bool checkFirstMessage);
        IBlockMonitoringService BlockMonitoringService { get; set; }
    }
}