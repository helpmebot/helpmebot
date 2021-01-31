namespace Helpmebot.ChannelServices.Commands.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("blockmonitor")]
    [CommandFlag(Flags.LocalConfiguration)]
    [CommandFlag(Flags.Configuration, true)]
    public class BlockMonitorConfigurationCommand : CommandBase
    {
        private readonly IBlockMonitoringService blockMonitoringService;
        private readonly ISession databaseSession;

        public BlockMonitorConfigurationCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IBlockMonitoringService blockMonitoringService,
            ISession databaseSession) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.blockMonitoringService = blockMonitoringService;
            this.databaseSession = databaseSession;
        }

        [SubcommandInvocation("add")]
        [Help("<channel>", "Adds monitoring of blocked users joining the specified channel, reporting in the current channel")]
        protected IEnumerable<CommandResponse> AddMode()
        {
            this.blockMonitoringService.AddMap(this.Arguments.First(), this.CommandSource, this.databaseSession);

            yield return new CommandResponse {Message = "Done"};
        }

        [SubcommandInvocation("del")]
        [SubcommandInvocation("delete")]
        [SubcommandInvocation("remove")]
        [Help("<channel>", "Removes monitoring of blocked users joining the specified channel, reporting in the current channel")]
        protected IEnumerable<CommandResponse> DeleteMode()
        {
            this.blockMonitoringService.DeleteMap(this.Arguments.First(), this.CommandSource, this.databaseSession);

            yield return new CommandResponse {Message = "Done"};
        }
    }
}