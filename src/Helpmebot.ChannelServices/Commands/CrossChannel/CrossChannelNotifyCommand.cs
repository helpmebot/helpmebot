namespace Helpmebot.ChannelServices.Commands.CrossChannel
{
    using System.Collections.Generic;
    using System.Data;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Standard)]
    public class CrossChannelNotifyCommand : CommandBase
    {
        private readonly ICrossChannelService crossChannelService;

        public CrossChannelNotifyCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ICrossChannelService crossChannelService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.crossChannelService = crossChannelService;
        }

        protected override IEnumerable<CommandResponse> Execute()
        {
            this.crossChannelService.Notify(
                this.CommandSource,
                this.OriginalArguments,
                this.Client,
                this.User);
            
            yield break;
        }
    }
}