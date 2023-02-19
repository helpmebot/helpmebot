namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Protected)]
    [CommandInvocation("forcewelcome")]
    [CommandInvocation("fwelcome")]
    [CommandInvocation("welcomef")]
    [HelpSummary("Forces the welcomer to trigger for the provided nickname, giving them the standard welcome message.")]
    public class ForceWelcomeCommand : CommandBase
    {
        private readonly IJoinMessageService joinMessageService;
        private readonly ICrossChannelService crossChannelService;

        public ForceWelcomeCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IJoinMessageService joinMessageService,
            ICrossChannelService crossChannelService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.joinMessageService = joinMessageService;
            this.crossChannelService = crossChannelService;
        }

        [RequiredArguments(1)]
        [Help("<nickname>")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            if (this.Client.Channels[this.CommandSource].Users.ContainsKey(this.Arguments[0]))
            {
                var user = this.Client.Channels[this.CommandSource].Users[this.Arguments[0]];
                
                this.joinMessageService.SendWelcome(user.User, this.CommandSource);
                yield break;
            }

            var frontendChannelName = this.crossChannelService.GetFrontendChannelName(this.CommandSource);
            if (frontendChannelName == null)
            {
                yield break;
            }

            if (!this.Client.Channels[frontendChannelName].Users.ContainsKey(this.Arguments[0]))
            {
                yield break;
            }

            var frontendUser = this.Client.Channels[frontendChannelName].Users[this.Arguments[0]];
            this.joinMessageService.SendWelcome(frontendUser.User, frontendChannelName);
        }
    }
}