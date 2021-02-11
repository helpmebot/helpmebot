namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System;
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
    using Stwalkerster.IrcClient.Messages;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Owner)]
    [CommandInvocation("botops")]
    [Obsolete]
    [Undocumented]
    public class BotOpsCommand : CommandBase
    {
        private readonly IPersistentChanOpsService persistentChanOpsService;
        private readonly IModeMonitoringService modeMonitoringService;

        public BotOpsCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IPersistentChanOpsService persistentChanOpsService,
            IModeMonitoringService modeMonitoringService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.persistentChanOpsService = persistentChanOpsService;
            this.modeMonitoringService = modeMonitoringService;
        }

        [SubcommandInvocation("grant")]
        protected IEnumerable<CommandResponse> GrantCommand()
        {
            this.persistentChanOpsService.RequestOps(this.CommandSource);
            yield break;
        }
        
        [SubcommandInvocation("revoke")]
        protected IEnumerable<CommandResponse> RevokeCommand()
        {
            this.persistentChanOpsService.ReleaseOps(this.CommandSource);
            yield break;
        }
        
        [SubcommandInvocation("topic")]
        [CommandFlag("z")]
        protected IEnumerable<CommandResponse> TopicCommand()
        {
            this.modeMonitoringService.PerformAsOperator(
                this.CommandSource,
                client => client.Send(
                    new Message("TOPIC", new[] {this.CommandSource, string.Join(" ", this.Arguments)})));
            yield break;
        }
    }
}