namespace Helpmebot.Commands.Commands.FunCommands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [HelpCategory("Fun")]
    public abstract class FunCommandBase : CommandBase
    {
        private readonly IChannelManagementService channelManagementService;
        public IResponder Responder { get; }

        protected FunCommandBase(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.channelManagementService = channelManagementService;
            this.Responder = responder;
        }

        protected override IEnumerable<CommandResponse> OnPreRun(out bool abort)
        {
            abort = this.channelManagementService.FunCommandsDisabled(this.CommandSource);
        
            if (abort)
            {
                return this.Responder.Respond("funcommands.disabled", this.CommandSource, this.CommandSource);
            }

            return null;
        }
    }
}