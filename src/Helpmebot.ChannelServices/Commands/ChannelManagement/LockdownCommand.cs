namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
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

    [CommandFlag(Flags.ChanOp)]
    [CommandInvocation("ld")]
    [Undocumented]
    public class LockdownCommand : CommandBase
    {
        private readonly IModeMonitoringService modeMonitoringService;

        public LockdownCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IModeMonitoringService modeMonitoringService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.modeMonitoringService = modeMonitoringService;
        }

        [Help("", "Enables channel lockdown mode, quieting all unregistered users and exempting as configured in the welcomer.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            try
            {
                var channel = "#wikipedia-en-help";
                var bantracker = "litharge";

                var chanop = this.User;
                
                this.modeMonitoringService.PerformAsOperator(
                    channel,
                    ircClient =>
                    {
                        ircClient.Mode(channel, $"+q $~a");
                        ircClient.Mode(channel, $"+o {chanop.Nickname}");
                        
                        // allow time for eir to message us
                        Thread.Sleep(1000);
                        ircClient.SendMessage(bantracker, $"2h Channel lockdown triggered by {chanop}");
                    });
            }
            catch (Exception ex)
            {
                return new[] {new CommandResponse {Message = ex.Message}};
            }

            return null;
        }
    }
}