namespace Helpmebot.ChannelServices.Services.Interfaces
{
    using Helpmebot.ChannelServices.Model;

    public interface IChannelOperator
    {
        void OnChannelOperatorGranted(object sender, OppedEventArgs e);
    }
}