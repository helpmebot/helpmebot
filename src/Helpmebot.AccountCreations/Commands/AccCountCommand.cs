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
            
            var created = nav.SelectSingleNode("//user/@created").Value;
            var today = nav.SelectSingleNode("//user/@today").Value;
            var suspended = nav.SelectSingleNode("//user/@suspended").Value;
            var promoted = nav.SelectSingleNode("//user/@promoted").Value;
            var approved = nav.SelectSingleNode("//user/@approved").Value;
            var demoted = nav.SelectSingleNode("//user/@demoted").Value;
            var declined = nav.SelectSingleNode("//user/@declined").Value;
            var renamed = nav.SelectSingleNode("//user/@renamed").Value;
            var edited = nav.SelectSingleNode("//user/@edited").Value;
            var prefchange = nav.SelectSingleNode("//user/@prefchange").Value;
            
            var adminCount = suspended + promoted + approved + demoted + declined + renamed + edited + prefchange;

            object[] messageParams =
            {
                username, // username
                "User",
                created,
                today,
                string.Empty
            };

            var messageKey = "accountcreations.command.count";

            if (adminCount > 0)
            {
                messageKey = "accountcreations.command.count.admin";
                messageParams = new object[]
                {
                    username, // username
                    "Admin",
                    created,
                    today,
                    suspended,
                    promoted,
                    approved,
                    demoted,
                    declined,
                    renamed,
                    edited,
                    prefchange
                };
            }

            return this.responder.Respond(messageKey, this.CommandSource, messageParams);
        }
    }
}