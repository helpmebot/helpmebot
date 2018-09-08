namespace Helpmebot.Commands.ACC
{
    using System.Collections.Generic;
    using System.Net;
    using System.Xml.XPath;
    using Castle.Core.Logging;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;
    using HttpRequest = Helpmebot.HttpRequest;

    [CommandInvocation("accstatus")]
    [CommandFlag(Flags.Acc)]
    public class AccStatusCommand : CommandBase
    {
        private readonly IMessageService messageService;

        public AccStatusCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMessageService messageService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.messageService = messageService;
        }

        [Help("", "Reports the current status of the ACC tool")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            string httpResponseData;

            try
            {
                httpResponseData = HttpRequest.Get("https://accounts.wmflabs.org/api.php?action=status");
            }
            catch (WebException e)
            {
                this.Logger.Warn("Error getting remote data", e);
                return new[] {new CommandResponse {Message = e.Message}};
            }

            var nav = new XPathDocument(httpResponseData.ToStream()).CreateNavigator();

            string[] messageParams =
            {
                nav.SelectSingleNode("//status/@open").Value,
                nav.SelectSingleNode("//status/@admin").Value,
                nav.SelectSingleNode("//status/@checkuser").Value,
                nav.SelectSingleNode("//status/@hold").Value,
                nav.SelectSingleNode("//status/@proxy").Value,

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