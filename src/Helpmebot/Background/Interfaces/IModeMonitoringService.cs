namespace Helpmebot.Background.Interfaces
{
    using Castle.Core;

    public interface IModeMonitoringService : IStartable
    {
        void ResyncChannel(string channel);
    }
}