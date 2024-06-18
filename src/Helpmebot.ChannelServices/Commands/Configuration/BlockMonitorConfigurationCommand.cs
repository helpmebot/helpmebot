namespace Helpmebot.ChannelServices.Commands.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("blockmonitor")]
    [CommandFlag(Flags.Configuration, true)]
    [HelpSummary("Manages configuration of on-wiki block reporting")]
    public class BlockMonitorConfigurationCommand : CommandBase
    {
        private readonly IBlockMonitoringService blockMonitoringService;
        private readonly ISession databaseSession;
        private readonly IResponder responder;

        public BlockMonitorConfigurationCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IBlockMonitoringService blockMonitoringService,
            ISession databaseSession,
            IResponder responder) : base(
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
            this.responder = responder;
        }

        [SubcommandInvocation("add")]
        [Help("<channel>", "Adds monitoring of blocked users joining the specified channel, reporting in the current channel")]
        protected IEnumerable<CommandResponse> AddMode()
        {
            this.blockMonitoringService.AddMap(this.Arguments.First(), this.CommandSource, this.databaseSession);

            return this.responder.Respond("common.done", this.CommandSource);
        }

        [SubcommandInvocation("del")]
        [SubcommandInvocation("delete")]
        [SubcommandInvocation("remove")]
        [Help("<channel>", "Removes monitoring of blocked users joining the specified channel, reporting in the current channel")]
        protected IEnumerable<CommandResponse> DeleteMode()
        {
            this.blockMonitoringService.DeleteMap(this.Arguments.First(), this.CommandSource, this.databaseSession);

            return this.responder.Respond("common.done", this.CommandSource);
        }
    }
}