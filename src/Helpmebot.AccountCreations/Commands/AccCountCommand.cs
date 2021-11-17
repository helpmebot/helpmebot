namespace Helpmebot.AccountCreations.Commands
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
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Acc)]
    [CommandInvocation("acccount")]
    [HelpCategory("ACC")]
    public class AccCountCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IWebServiceClient webServiceClient;
        private readonly BotConfiguration botConfiguration;

        public AccCountCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
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
            this.responder = responder;
            this.webServiceClient = webServiceClient;
            this.botConfiguration = botConfiguration;
        }

        [Help("[username]", "Provides statistics on the number of ACC requests closed by the provided user")]
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
                {"action", "count"},
                {"user", username}
            };

            Stream httpResponseData;
            try
            {
                httpResponseData = this.webServiceClient.DoApiCall(
                    queryParameters,
                    "https://accounts.wmflabs.org/api.php",
                    this.botConfiguration
                        .UserAgent);
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
                return this.responder.Respond(
                    "accountcreations.no-such-user",
                    this.CommandSource,
                    username);
            }

            var userLevelNode = nav.SelectSingleNode("//user/@level");

            object[] messageParams =
            {
                username, // username
                userLevelNode.Value,
                nav.SelectSingleNode("//user/@created").Value,
                nav.SelectSingleNode("//user/@today").Value,
                string.Empty
            };

            var messageKey = "accountcreations.command.count";

            if (userLevelNode.Value == "Admin")
            {
                messageKey = "accountcreations.command.count.admin";
                messageParams = new object[]
                {
                    username, // username
                    userLevelNode.Value,
                    nav.SelectSingleNode("//user/@created").Value,
                    nav.SelectSingleNode("//user/@today").Value,
                    nav.SelectSingleNode("//user/@suspended").Value,
                    nav.SelectSingleNode("//user/@promoted").Value,
                    nav.SelectSingleNode("//user/@approved").Value,
                    nav.SelectSingleNode("//user/@demoted").Value,
                    nav.SelectSingleNode("//user/@declined").Value,
                    nav.SelectSingleNode("//user/@renamed").Value,
                    nav.SelectSingleNode("//user/@edited").Value,
                    nav.SelectSingleNode("//user/@prefchange").Value
                };
            }

            return this.responder.Respond(messageKey, this.CommandSource, messageParams);
        }
    }
}