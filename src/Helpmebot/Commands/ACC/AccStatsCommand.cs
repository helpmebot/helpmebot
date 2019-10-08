namespace Helpmebot.Commands.ACC
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.XPath;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.Configuration;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("accstats")]
    [CommandFlag(Flags.Acc)]
    [HelpCategory("ACC")]
    public class AccStatsCommand : CommandBase
    {
        private readonly IMessageService messageService;
        private readonly IWebServiceClient webServiceClient;
        private readonly BotConfiguration botConfiguration;

        public AccStatsCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMessageService messageService,
            IWebServiceClient webServiceClient,
            BotConfiguration botConfiguration) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.messageService = messageService;
            this.webServiceClient = webServiceClient;
            this.botConfiguration = botConfiguration;
        }

        [Help("[username]", "Provides information on an ACC user")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            string[] args = this.Arguments.ToArray();

            string username;

            if (args.Length > 0 && args[0] != string.Empty)
            {
                username = string.Join(" ", args);
            }
            else
            {
                username = this.User.Nickname;
            }

            var queryParameters = new NameValueCollection
            {
                {"action", "stats"},
                {"user", username}
            };

            Stream httpResponseData;
            try
            {
                httpResponseData = this.webServiceClient.DoApiCall(
                    queryParameters,
                    "https://accounts.wmflabs.org/api.php",
                    this.botConfiguration.UserAgent);
            }
            catch (WebException e)
            {
                this.Logger.Warn("Error getting remote data", e);

                return new[] {new CommandResponse {Message = e.Message}};
            }

            var nav = new XPathDocument(httpResponseData).CreateNavigator();

            var isMissing = nav.SelectSingleNode("//user/@missing") != null;
            if (isMissing)
            {
                var msg = this.messageService.RetrieveMessage("noSuchUser", this.CommandSource, new[] {username});
                return new[] {new CommandResponse {Message = msg}};
            }

            string[] messageParams =
            {
                username, // username
                nav.SelectSingleNode("//user/@status").Value, // accesslevel
                nav.SelectSingleNode("//user/@lastactive").Value,
                nav.SelectSingleNode("//user/@welcome_template").Value == string.Empty
                    ? "disabled"
                    : "enabled",
                nav.SelectSingleNode("//user/@onwikiname").Value
            };

            var message = this.messageService.RetrieveMessage("CmdAccStats", this.CommandSource, messageParams);
            return new[] {new CommandResponse {Message = message}};
        }
    }
}