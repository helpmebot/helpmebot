namespace Helpmebot.Commands.ACC
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using Castle.Core.Logging;
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

    [CommandInvocation("accdeploy")]
    [CommandFlag(Flags.Acc)]
    public class AccDeployCommand : CommandBase
    {
        private readonly IMessageService messageService;
        private readonly BotConfiguration botConfiguration;
        private readonly IWebServiceClient webServiceClient;

        public AccDeployCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMessageService messageService,
            BotConfiguration botConfiguration,
            IWebServiceClient webServiceClient) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.messageService = messageService;
            this.botConfiguration = botConfiguration;
            this.webServiceClient = webServiceClient;
        }

        [RequiredArguments(1)]
        [Help("<branch>", "Deploys the specified branch to the ACC sandbox environment")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var apiDeployPassword = this.botConfiguration.AccDeploymentPassword;
            if (apiDeployPassword == null)
            {
                yield return new CommandResponse
                {
                    Message = "Deployment disabled in configuration",
                    Destination = CommandResponseDestination.PrivateMessage,
                    Type = CommandResponseType.Notice
                };

                yield break;
            }

            var args = this.Arguments;

            // note: using client.sendmessage for immediacy
            var deployInProgressMessage =
                this.messageService.RetrieveMessage("DeployInProgress", this.CommandSource, null);
            this.Client.SendMessage(this.CommandSource, deployInProgressMessage);

            var revision = string.Join(" ", args);
            var key = this.EncodeMD5(this.EncodeMD5(revision) + apiDeployPassword);

            var queryParameters = new NameValueCollection
            {
                {"r", revision},
                {"k", key}
            };

            using (var data = this.webServiceClient.DoApiCall(
                queryParameters,
                "https://accounts-dev.wmflabs.org/deploy/deploy.php",
                this.botConfiguration
                    .UserAgent))
            {
                var r = new StreamReader(data);

                while (!r.EndOfStream)
                {
                    this.Client.SendMessage(this.CommandSource, r.ReadLine());
                }
            }
        }

        private string EncodeMD5(string s)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(new System.Text.UTF8Encoding().GetBytes(s));

            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
    }
}