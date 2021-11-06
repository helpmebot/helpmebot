namespace Helpmebot.WebApi.Services.Interfaces
{
    using Helpmebot.WebApi.TransportModels;

    public interface IApiService
    {
        BotStatus GetBotStatus();
    }
}