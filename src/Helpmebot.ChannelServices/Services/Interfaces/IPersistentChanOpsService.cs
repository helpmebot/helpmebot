namespace Helpmebot.ChannelServices.Services.Interfaces
{
    using System;

    [Obsolete]
    public interface IPersistentChanOpsService
    {
        void RequestOps(string channel);
        void ReleaseOps(string channel);
    }
}