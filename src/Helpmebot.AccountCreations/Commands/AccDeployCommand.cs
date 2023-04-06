namespace Helpmebot.AccountCreations.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
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
    using ModuleConfiguration = Helpmebot.AccountCreations.Configuration.ModuleConfiguration;

    [CommandInvocation("accdeploy")]
    [CommandFlag(Flags.Acc)]
    [HelpCategory("ACC")]
    [HelpSummary("Utility command for ACC tool development deployment")]
    public class AccDeployCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly BotConfiguration botConfiguration;
        private readonly IWebServiceClient webServiceClient;
        private readonly string deploymentPassword;

        public AccDeployCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
            BotConfiguration botConfiguration,
            IWebServiceClient webServiceClient,
            ModuleConfiguration deploymentConfiguration) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
            this.botConfiguration = botConfiguration;
            this.webServiceClient = webServiceClient;
            this.deploymentPassword = deploymentConfiguration.DeploymentPassword;
        }

        [RequiredArguments(1)]
        [Help("<branch>", "Deploys the specified branch to the ACC sandbox environment")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var apiDeployPassword = this.deploymentPassword;
            if (apiDeployPassword == null)
            {
                return this.responder.Respond(
                    "accountcreations.command.deploy.disabled",
                    this.CommandSource,
                    destination: CommandResponseDestination.PrivateMessage,
                    type: CommandResponseType.Notice);
            }

            var args = this.Arguments;

            var destination = this.Client.Nickname == this.CommandSource ? this.User.Nickname : this.CommandSource;

            // note: using client.sendmessage for immediacy
            foreach (var response in this.responder.Respond(
                "accountcreations.command.deploy.inprogress",
                this.CommandSource))
            {
                this.Client.SendMessage(destination, response.CompileMessage());
            }
            
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
                this.botConfiguration.UserAgent))
            {
                var r = new StreamReader(data);

                while (!r.EndOfStream)
                {
                    this.Client.SendMessage(destination, r.ReadLine());
                }
            }

            return null;
        }

        private string EncodeMD5(string s)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(new System.Text.UTF8Encoding().GetBytes(s));

            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
    }
}