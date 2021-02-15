namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Configuration)]
    [Undocumented]
    public class EnactBanCommand : CommandBase
    {
        private readonly ITrollMonitoringService trollMonitoringService;

        public EnactBanCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ITrollMonitoringService trollMonitoringService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.trollMonitoringService = trollMonitoringService;
        }

        protected override IEnumerable<CommandResponse> Execute()
        {
            this.trollMonitoringService.EnactBan(this.User);
            yield break;
        }
    }
}