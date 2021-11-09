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
                Nickname = "FakeBot", IrcServer = "fake.irc.server", IrcServerPort = 6667
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