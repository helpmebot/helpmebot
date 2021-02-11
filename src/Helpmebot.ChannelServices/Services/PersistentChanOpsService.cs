namespace Helpmebot.ChannelServices.Services
{
    using System;
    using Helpmebot.ChannelServices.Model;
    using Helpmebot.ChannelServices.Services.Interfaces;

    [Obsolete]
    public class PersistentChanOpsService : IPersistentChanOpsService, IChannelOperator
    {
        private readonly IModeMonitoringService modeMonitoringService;

        public PersistentChanOpsService(IModeMonitoringService modeMonitoringService)
        {
            this.modeMonitoringService = modeMonitoringService;
        }
        
        public void RequestOps(string channel)
        {
            this.modeMonitoringService.RequestPersistentOps(channel, this, "abc");
        }
        
        public void ReleaseOps(string channel)
        {
            this.modeMonitoringService.ReleasePersistentOps(channel, "abc");
        }

        public void OnChannelOperatorGranted(object sender, OppedEventArgs e)
        {
        }
    }
}