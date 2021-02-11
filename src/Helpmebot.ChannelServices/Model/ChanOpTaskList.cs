namespace Helpmebot.ChannelServices.Model
{
    using System;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;

    public class ChanOpTaskList : IChannelOperator
    {
        private readonly Action<IIrcClient> task;

        public ChanOpTaskList(Action<IIrcClient> task)
        {
            this.task = task;
        }
        
        public void OnChannelOperatorGranted(object sender, OppedEventArgs e)
        {
            this.task(e.IrcClient);
            
            e.ModeMonitoringService.ReleasePersistentOps(e.Channel, e.Token);
        }
    }
}