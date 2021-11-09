namespace Helpmebot.WebUI.Services.Api
{
    using System.Collections.Generic;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;

    public class FakeApiService : IApiService

    {
        public BotStatus GetBotStatus()
        {
            return new BotStatus
            {
                ChannelCount = 123, Commands = new List<string> { "potato", "helpme", "carrot", "myflags", "link" },
                Nickname = "Helpmebot_", IrcServer = "irc.libera.chat", IrcServerPort = 6667, VisibleUserCount = 9999,
                PingTime = 130, TotalMessages = 10232
            };
        }

        public string GetLoginToken()
        {
            return "123token456";
        }

        public TokenResponse GetAuthToken(string loginToken)
        {
            return new TokenResponse { IrcAccount = "fakeuser", Token = loginToken };
        }
    }
}