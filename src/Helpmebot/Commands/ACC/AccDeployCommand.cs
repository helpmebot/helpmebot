namespace Helpmebot.Commands.ACC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
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

    [CommandInvocation("accdeploy")]
    [CommandFlag(Flags.Acc)]
    public class AccDeployCommand :CommandBase
    {
        private readonly IMessageService messageService;
        private readonly BotConfiguration botConfiguration;

        public AccDeployCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMessageService messageService,
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
            this.botConfiguration = botConfiguration;
        }

        [RequiredArguments(1)]
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
            var deployInProgressMessage = this.messageService.RetrieveMessage("DeployInProgress", this.CommandSource, null);
            this.Client.SendMessage(this.CommandSource, deployInProgressMessage);

            var revision = string.Join(" ", args);
            var key = this.EncodeMD5(this.EncodeMD5(revision) + apiDeployPassword);
            
            revision = HttpUtility.UrlEncode(revision);
            
            var requestUri = "https://accounts-dev.wmflabs.org/deploy/deploy.php?r=" + revision + "&k=" + key;

            using (var data = HttpRequest.Get(requestUri, 1000 * 30 /* 30 sec timeout */).ToStream())
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