namespace Helpmebot.WebApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Brain.Services.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Startup;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;

    public class ApiService : IApiService
    {
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly ICommandParser commandParser;
        private readonly BotConfiguration botConfiguration;
        private readonly IIrcConfiguration ircConfiguration;
        private readonly ILoginTokenService loginTokenService;

        public ApiService(ILogger logger, IIrcClient client, ICommandParser commandParser, BotConfiguration botConfiguration, IIrcConfiguration ircConfiguration, ILoginTokenService loginTokenService)
        {
            this.logger = logger;
            this.client = client;
            this.commandParser = commandParser;
            this.botConfiguration = botConfiguration;
            this.ircConfiguration = ircConfiguration;
            this.loginTokenService = loginTokenService;
        }
        
        public IKeywordService BrainKeywordService { get; set; }
        
        public BotStatus GetBotStatus()
        {
            return new BotStatus
            {
                ChannelCount = this.client.Channels.Count,
                Commands = this.commandParser.GetCommandRegistrations().Keys.ToList(),
                Nickname = this.client.Nickname,
                Trigger = this.botConfiguration.CommandTrigger,
                IrcServer = this.ircConfiguration.Hostname,
                IrcServerPort = this.ircConfiguration.Port,
                PingTime = this.client.Latency,
                StartupTime = Launcher.StartupTime,
                TotalMessages = this.client.PrivmsgReceived,
                VisibleUserCount = this.client.UserCache.Count
            };
        }

        public string GetLoginToken()
        {
            return this.loginTokenService.GetLoginToken();
        }

        public TokenResponse GetAuthToken(string loginToken)
        {
            return this.loginTokenService.GetAuthToken(loginToken);
        }

        public List<BrainItem> GetBrainItems()
        {
            if (this.BrainKeywordService == null)
            {
                this.logger.Warn("GetBrainItems called but BrainKeywordService is null.");
                throw new Exception("Missing API dependency");
            }

            return this.BrainKeywordService.GetAll()
                .Select(x => new BrainItem { IsAction = x.Action, Keyword = x.Name, Response = x.Response })
                .ToList();
        }
    }
}