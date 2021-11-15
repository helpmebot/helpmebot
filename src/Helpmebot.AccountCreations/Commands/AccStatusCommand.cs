namespace Helpmebot.AccountCreations.Commands
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Xml.XPath;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("accstatus")]
    [CommandFlag(Flags.Acc)]
    [HelpCategory("ACC")]
    public class AccStatusCommand : CommandBase
    {
        private readonly IMessageService messageService;
        private readonly IWebServiceClient webServiceClient;
        private readonly BotConfiguration botConfiguration;

        public AccStatusCommand(
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

        [Help("", "Reports the current status of the ACC tool")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var queryParameters = new NameValueCollection
            {
                {"action", "status"}
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

            string[] messageParams =
            {
                nav.SelectSingleNode("//status/@open").Value,
                nav.SelectSingleNode("//status/@admin").Value,
                nav.SelectSingleNode("//status/@checkuser").Value,
                nav.SelectSingleNode("//status/@hold").Value,
                nav.SelectSingleNode("//status/@proxy").Value,
                nav.SelectSingleNode("//status/@steward").Value,
                nav.SelectSingleNode("//status/@x-hospital").Value,
                nav.SelectSingleNode("//status/@x-jobqueue").Value,

                nav.SelectSingleNode("//status/@bans").Value,

                nav.SelectSingleNode("//status/@useradmin").Value,
                nav.SelectSingleNode("//status/@user").Value,
                nav.SelectSingleNode("//status/@usernew").Value
            };

            var message = this.messageService.RetrieveMessage("CmdAccStatus", this.CommandSource, messageParams);
            return new[] {new CommandResponse {Message = message}};
        }
    }
}