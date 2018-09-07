namespace Helpmebot.Services.Interfaces
{
    using NHibernate;
    using Stwalkerster.IrcClient.Events;

    public interface IChannelManagementService
    {
        void OnInvite(object sender, InviteEventArgs e);
        void OnKicked(object sender, KickedEventArgs e);
        void JoinChannel(string channelName, ISession localSession);
        void PartChannel(string channelName, ISession localSession, string message);
    }
}