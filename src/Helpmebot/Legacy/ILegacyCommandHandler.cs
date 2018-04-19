namespace Helpmebot.Legacy
{
    using Stwalkerster.IrcClient.Events;

    public interface ILegacyCommandHandler
    {
        void ReceivedMessage(object sender, MessageReceivedEventArgs ea);
    }
}