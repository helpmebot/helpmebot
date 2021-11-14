namespace Helpmebot.WebApi.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Helpmebot.WebApi.TransportModels;

    public interface IApiService
    {
        BotStatus GetBotStatus();
        
        string GetLoginToken();
        
        TokenResponse GetAuthToken(string loginToken);

        List<BrainItem> GetBrainItems();

        void InvalidateToken(string authTokenHandle);

        List<CommandInfo> GetRegisteredCommands();

        Dictionary<string, Tuple<string, string>> GetFlagHelp();

        List<FlagGroup> GetFlagGroups();
    }
}