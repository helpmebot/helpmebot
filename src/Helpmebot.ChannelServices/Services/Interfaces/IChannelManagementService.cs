namespace Helpmebot.ChannelServices.Services.Interfaces
{
    using Stwalkerster.IrcClient.Events;

    public interface IChannelManagementService
    {
        void OnInvite(object sender, InviteEventArgs e);
        void OnKicked(object sender, KickedEventArgs e);
        void JoinChannel(string channelName);
        void PartChannel(string channelName, string message);
    }
}