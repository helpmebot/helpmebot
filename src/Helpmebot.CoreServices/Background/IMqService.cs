namespace Helpmebot.CoreServices.Background
{
    using Castle.Core;
    using RabbitMQ.Client;

    public interface IMqService : IStartable
    {
        IModel CreateChannel();
        void ReturnChannel(IModel channel);
    }
}