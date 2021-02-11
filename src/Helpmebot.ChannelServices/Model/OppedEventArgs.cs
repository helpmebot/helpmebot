namespace Helpmebot.ChannelServices.Model
{
    using System;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;

    public class OppedEventArgs : EventArgs
    {
        public IIrcClient IrcClient { get; }
        public string Channel { get; }
        public string Token { get; }
        public IModeMonitoringService ModeMonitoringService { get; }
        
        public OppedEventArgs(string channel, string token, IModeMonitoringService modeMonitoringService, IIrcClient ircClient)
        {
            this.IrcClient = ircClient;
            this.Channel = channel;
            this.Token = token;
            this.ModeMonitoringService = modeMonitoringService;
        }
    }
}