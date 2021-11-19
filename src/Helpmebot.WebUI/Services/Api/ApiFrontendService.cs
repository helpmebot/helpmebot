namespace Helpmebot.WebUI.Services.Api
{
    using System;
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
        
        public void InvalidateToken(string authTokenHandle)
        {
            this.transportService.RemoteProcedureCall<string, object>(MethodBase.GetCurrentMethod(), authTokenHandle);
        }

        public List<CommandInfo> GetRegisteredCommands()
        {
            return this.transportService.RemoteProcedureCall<object, List<CommandInfo>>(MethodBase.GetCurrentMethod(), null);
        }

        public Dictionary<string, Tuple<string, string>> GetFlagHelp()
        {
            return this.transportService.RemoteProcedureCall<object, Dictionary<string, Tuple<string, string>>>(MethodBase.GetCurrentMethod(), null);
        }

        public List<FlagGroup> GetFlagGroups()
        {
            return this.transportService.RemoteProcedureCall<object, List<FlagGroup>>(MethodBase.GetCurrentMethod(), null);
        }

        public AccessControlList GetAccessControlList()
        {
            return this.transportService.RemoteProcedureCall<object, AccessControlList>(MethodBase.GetCurrentMethod(), null);
        }

        public List<InterwikiPrefix> GetInterwikiList()
        {
            return this.transportService.RemoteProcedureCall<object, List<InterwikiPrefix>>(MethodBase.GetCurrentMethod(), null);
        }

        public List<string> GetMessageKeys()
        {
            return this.transportService.RemoteProcedureCall<object, List<string>>(MethodBase.GetCurrentMethod(), null);
        }

        public Response GetMessageDefaultResponses(ResponseKey key)
        {
            return this.transportService.RemoteProcedureCall<ResponseKey, Response>(MethodBase.GetCurrentMethod(), key);
        }

        public TokenResponse GetAuthToken(string loginToken)
        {
            return this.transportService.RemoteProcedureCall<string, TokenResponse>(MethodBase.GetCurrentMethod(), loginToken);
        }

        public List<BrainItem> GetBrainItems()
        {
            return this.transportService.RemoteProcedureCall<object, List<BrainItem>>(MethodBase.GetCurrentMethod(), null);
        }

        public List<Response> GetMessageResponses()
        {
            return this.transportService.RemoteProcedureCall<object, List<Response>>(MethodBase.GetCurrentMethod(), null);
        }
    }
}