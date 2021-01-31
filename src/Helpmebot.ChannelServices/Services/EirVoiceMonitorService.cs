namespace Helpmebot.ChannelServices.Services
{
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Model.ModeMonitoring;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;

    public class EirVoiceMonitorService : IEirVoiceMonitorService
    {
        private readonly IIrcClient client;
        private readonly ILogger logger;

        public EirVoiceMonitorService(IIrcClient client, ILogger logger)
        {
            this.client = client;
            this.logger = logger;
        }

        private void ClientOnModeReceivedEvent(object sender, ModeEventArgs e)
        {
            if (e.Target == "#wikipedia-en-help")
            {
                var v = ModeChanges.FromChangeList(e.Changes);
                if (v.Voices.Contains("eir"))
                {
                    this.client.SendMessage("ChanServ", "devoice #wikipedia-en-help eir");
                    this.client.SendMessage("#wikipedia-en-helpers", "Heads-up: I noticed eir (freenode's bantracker bot) was voiced in -en-help. I've automatically devoiced it again, due to the potential problems which can be caused by leaving it voiced. Please speak to stwalkerster or any other channel op for more information.");
                }
            }
        }

        public void Start()
        {
            this.logger.Debug("Starting voice monitor service");
            this.client.ModeReceivedEvent += this.ClientOnModeReceivedEvent; 
        }

        public void Stop()
        {
            this.logger.Debug("Stopping voice monitor service");
            this.client.ModeReceivedEvent -= this.ClientOnModeReceivedEvent; 
        }
    }
}