namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.Model;
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
    public class ForceWelcomeCommand : CommandBase
    {
        private readonly IJoinMessageService joinMessageService;

        public ForceWelcomeCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IJoinMessageService joinMessageService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.joinMessageService = joinMessageService;
        }

        [RequiredArguments(1)]
        [Help("<nickname>", "Forces the welcomer to trigger for the provided nickname, giving them the standard welcome message.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            if (this.Client.Channels[this.CommandSource].Users.ContainsKey(this.Arguments[0]))
            {
                var user = this.Client.Channels[this.CommandSource].Users[this.Arguments[0]];
                
                this.joinMessageService.SendWelcome(user.User, this.CommandSource, this.Client);
            }
            
            yield break;
        }
    }
}