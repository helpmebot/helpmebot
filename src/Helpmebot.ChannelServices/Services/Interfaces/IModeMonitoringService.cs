namespace Helpmebot.ChannelServices.Services.Interfaces
{
    using System;
    using Castle.Core;
    using Stwalkerster.IrcClient.Interfaces;

    public interface IModeMonitoringService : IStartable
    {
        void ResyncChannel(string channel);
        void RequestPersistentOps(string channel, IChannelOperator requester, string token);
        void ReleasePersistentOps(string channel, string token);
        void PerformAsOperator(string channel, Action<IIrcClient> tasks, bool priority);
        void PerformAsOperator(string channel, Action<IIrcClient> tasks);
    }
}