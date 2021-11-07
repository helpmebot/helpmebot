namespace Helpmebot.WebApi.Services
{
    using System.Linq;
    using Helpmebot.Configuration;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using YamlDotNet.Core.Tokens;

    public class ApiService : IApiService
    {
        private readonly IIrcClient client;
        private readonly ICommandParser commandParser;
        private readonly BotConfiguration botConfiguration;
        private readonly IIrcConfiguration ircConfiguration;
        private readonly ILoginTokenService loginTokenService;

        public ApiService(IIrcClient client, ICommandParser commandParser, BotConfiguration botConfiguration, IIrcConfiguration ircConfiguration, ILoginTokenService loginTokenService)
        {
            this.client = client;
            this.commandParser = commandParser;
            this.botConfiguration = botConfiguration;
            this.ircConfiguration = ircConfiguration;
            this.loginTokenService = loginTokenService;
        }
        
        public BotStatus GetBotStatus()
        {
            return new BotStatus
            {
                ChannelCount = this.client.Channels.Count,
                Commands = this.commandParser.GetCommandRegistrations().Keys.ToList(),
                Nickname = this.client.Nickname,
                Trigger = this.botConfiguration.CommandTrigger,
                IrcServer = this.ircConfiguration.Hostname,
                IrcServerPort = this.ircConfiguration.Port
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
    }
}