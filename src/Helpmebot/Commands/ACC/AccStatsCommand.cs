namespace Helpmebot.Commands.ACC
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
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

    [CommandInvocation("accstats")]
    [CommandFlag(Flags.Acc)]
    public class AccStatsCommand : CommandBase
    {
        private readonly IMessageService messageService;

        public AccStatsCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client, IMessageService messageService) : base(
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
            
            username = HttpUtility.UrlEncode(username);

            var uri = "https://accounts.wmflabs.org/api.php?action=stats&user=" + username;

            string httpResponseData;
            try
            {
                httpResponseData = HttpRequest.Get(uri);
            }
            catch (WebException e)
            {
                this.Logger.Warn("Error getting remote data", e);
                
                return new[] {new CommandResponse {Message = e.Message}};
            }
            
            var nav = new XPathDocument(httpResponseData.ToStream()).CreateNavigator();

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
                nav.SelectSingleNode("//user/@welcome_template").ValueAsInt == 0
                    ? "disabled"
                    : "enabled",
                nav.SelectSingleNode("//user/@onwikiname").Value
            };

            var message = this.messageService.RetrieveMessage("CmdAccStats", this.CommandSource, messageParams);
            return new[] {new CommandResponse {Message = message}};
        }
    }
}