namespace Helpmebot.AccountCreations.Commands
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.XPath;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.Attributes;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Acc)]
    [CommandInvocation("acccount")]
    [HelpCategory("ACC")]
    [HelpSummary("Provides statistics on the number of ACC requests closed")]
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

        [Help("[username]", "Returns stats for the given user, or the nickname of the current user if not specified")]
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
            
            var created = int.Parse(nav.SelectSingleNode("//user/@created").Value);
            var today = int.Parse(nav.SelectSingleNode("//user/@today").Value);
            var suspended = int.Parse(nav.SelectSingleNode("//user/@suspended").Value);
            var promoted = int.Parse(nav.SelectSingleNode("//user/@promoted").Value);
            var approved = int.Parse(nav.SelectSingleNode("//user/@approved").Value);
            var demoted = int.Parse(nav.SelectSingleNode("//user/@demoted").Value);
            var declined = int.Parse(nav.SelectSingleNode("//user/@declined").Value);
            var renamed = int.Parse(nav.SelectSingleNode("//user/@renamed").Value);
            var edited = int.Parse(nav.SelectSingleNode("//user/@edited").Value);
            var prefchange = int.Parse(nav.SelectSingleNode("//user/@prefchange").Value);
            
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