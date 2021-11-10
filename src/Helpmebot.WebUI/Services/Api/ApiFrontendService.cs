namespace Helpmebot.WebUI.Services.Api
{
    using System.Collections.Generic;
    using System.Reflection;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;
    using Microsoft.Extensions.Logging;

    public class ApiFrontendService : IApiService
    {
        private readonly IApiFrontendTransportService transportService;
        private readonly ILogger<ApiFrontendService> logger;

        public ApiFrontendService(IApiFrontendTransportService transportService, ILogger<ApiFrontendService> logger)
        {
            this.transportService = transportService;
            this.logger = logger;
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

        public List<BrainItem> GetBrainItems()
        {
            this.logger.LogDebug("Beginning BrainItems RPC");
            var rpcResult = this.transportService.RemoteProcedureCall<object, List<BrainItem>>(MethodBase.GetCurrentMethod(), null);
            this.logger.LogDebug("Done BrainItems RPC");
            return rpcResult;
            
        }
    }
}