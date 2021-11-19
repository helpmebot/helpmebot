namespace Helpmebot.WebUI.Services.Api
{
    using System;
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
                PingTime = 130, TotalMessages = 10232, BotVersion = "1.2.30"
            };
        }

        public string GetLoginToken()
        {
            return "123token456";
        }

        public void InvalidateToken(string _)
        {
        }

        public List<CommandInfo> GetRegisteredCommands()
        {
            return new List<CommandInfo>();
        }

        public Dictionary<string, Tuple<string, string>> GetFlagHelp()
        {
            return new Dictionary<string, Tuple<string, string>>();
        }

        public List<FlagGroup> GetFlagGroups()
        {
            return new List<FlagGroup>
            {
                new() {Name = "tet", Flags = "potato", Mode = "+", LastModified = DateTime.Now},
                new() {Name = "sdfs", Flags = "potaeefto", Mode = "-", LastModified = DateTime.Now},
                new() {Name = "Owner", Flags = "OWNERYAY", Mode = "+", LastModified = DateTime.Now},
                new() {Name = "erm?", Flags = "nope", Mode = "+", LastModified = DateTime.Now},
            };
        }

        public AccessControlList GetAccessControlList()
        {
            return new AccessControlList
            {
                Users = new List<UserAccessControlEntry>
                {
                    new() { AccountName = "stwalkerster", FlagGroups = new List<string> { "Owner", "Superuser" } },
                    new() { IrcMask = "*!*@wikimedia/*", FlagGroups = new List<string> { "Advanced" } },
                }
            };
        }

        public List<InterwikiPrefix> GetInterwikiList()
        {
            return new List<InterwikiPrefix>();
        }

        public List<string> GetMessageKeys()
        {
            return new List<string>();
        }

        public Response GetMessageDefaultResponses(ResponseKey key)
        {
            return new Response();
        }

        public List<Response> GetMessageResponses()
        {
            return new List<Response>();
        }

        public TokenResponse GetAuthToken(string loginToken)
        {
            return new TokenResponse { IrcAccount = "fakeuser", Token = loginToken };
        }

        public List<BrainItem> GetBrainItems()
        {
            return new List<BrainItem>
            {
                new() { Keyword = "commands", Response = "https://helpmebot.org.uk/wiki/Commands", IsAction = false},
                new() { Keyword = "accesslist", Response = "https://helpmebot.org.uk/wiki/Special:AccessList", IsAction = false},
                new() { Keyword = "brain", Response = "https://helpmebot.org.uk/wiki/Special:Brain", IsAction = false},
                new() { Keyword = "quote", Response = "http://meta.wikimedia.org/wiki/IRC/Quotes#Quotes", IsAction = false},
                new() { Keyword = "accgraph", Response = "http://accounts-dev.wmflabs.org/graph/", IsAction = false},
                new() { Keyword = "goldenrule", Response = "Articles require significant coverage in reliable sources that are independent of the subject. 12http://enwp.org/WP:ANS", IsAction = false},
                new() { Keyword = "cv", Response = "Do not add content to Wikipedia if you think that doing so may be a copyright violation. Contributors should take steps to remove any copyright violation that they find. 3http://enwp.org/WP:COPYVIO", IsAction = false},
                new() { Keyword = "formattingtest", Response = "test 0white test 1black test 2blue test 3green test 4light-red test 5brown test 6purple test 7orange test 8yellow test 9light-green test 10cyan test 11light-cyan test 12light-blue test 13pink test 14grey test 15light-grey test 16white test", IsAction = false},
                new() { Keyword = "formattingtest2", Response = "test ,0white test ,1black test ,2blue test ,3green test ,4light-red test ,5brown test ,6purple test ,7orange test ,8yellow test ,9light-green test ,10cyan test ,11light-cyan test ,12light-blue test ,13pink test ,14grey test ,15light-grey test ,16white test", IsAction = false},
                new() { Keyword = "formattingtest3", Response = "normal bold italic underline normal bold bolditalic bold normal", IsAction = false},
                new() { Keyword = "formattingtest4", Response = "normal 3bold italic underline normal bold bolditalic italic normal", IsAction = false},
                new() { Keyword = "formattingtest5", Response = "<h2>test!</h2>", IsAction = false},
            };
        }
    }
}