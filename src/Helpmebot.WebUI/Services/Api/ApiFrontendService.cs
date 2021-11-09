namespace Helpmebot.WebUI.Services.Api
{
    using System.Reflection;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;

    public class ApiFrontendService : IApiService
    {
        private readonly IApiFrontendTransportService transportService;

        public ApiFrontendService(IApiFrontendTransportService transportService)
        {
            this.transportService = transportService;
        }

        public BotStatus GetBotStatus()
        {
            return this.transportService.RemoteProcedureCall<object, BotStatus>(MethodBase.GetCurrentMethod(), null);
        }

        public string GetLoginToken()
        {
            return this.transportService.RemoteProcedureCall<object, string>(MethodBase.GetCurrentMethod(), null);
        }

        public TokenResponse GetAuthToken(string loginToken)
        {
            return this.transportService.RemoteProcedureCall<string, TokenResponse>(MethodBase.GetCurrentMethod(), loginToken);
        }
    }
}