namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Startup;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Messages;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Owner)]
    [CommandInvocation("idleremove")]
    public class IdleRemoveCommand : CommandBase
    {
        private readonly IHelpeeManagementService helpeeManagementService;
        private readonly IModeMonitoringService modeMonitoringService;

        public IdleRemoveCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IHelpeeManagementService helpeeManagementService,
            IModeMonitoringService modeMonitoringService)
            : base(commandSource, user, arguments, logger, flagService, configurationProvider, client)
        {
            this.helpeeManagementService = helpeeManagementService;
            this.modeMonitoringService = modeMonitoringService;
        }

        [SubcommandInvocation("dryrun")]
        protected IEnumerable<CommandResponse> DryRun()
        {
            var removableHelpees = this.GetRemovableHelpees();

            var helpees = string.Join(", ", removableHelpees);

            yield return new CommandResponse {Message = $"Proposing removal of: {helpees}"};
        }

        [SubcommandInvocation("remove")]
        protected IEnumerable<CommandResponse> Remove()
        {
            var removableHelpees = this.GetRemovableHelpees();
            
            var channel = "#wikipedia-en-help";
            var removeMessage = "Idle; if you still need assistance editing Wikipedia, please re-join";

            this.modeMonitoringService.PerformAsOperator(
                channel,
                ircClient =>
                {
                    foreach (var helpee in removableHelpees)
                    {
                        ircClient.Send(new Message("REMOVE", new[] {channel, helpee, removeMessage}));
                    }
                });

            return null;
        }

        private List<string> GetRemovableHelpees()
        {
            var removableHelpees = this.helpeeManagementService.Helpees
                .Where(
                    x =>
                    {
                        if (x.Value == DateTime.MinValue)
                        {
                            if ((DateTime.UtcNow - Launcher.StartupTime).TotalMinutes > 15)
                            {
                                return true;
                            }
                        }

                        if ((DateTime.UtcNow - x.Value).TotalMinutes > 15)
                        {
                            return true;
                        }

                        return false;
                    })
                .Select(x => x.Key.Nickname)
                .Intersect(this.Arguments)
                .ToList();
            return removableHelpees;
        }
    }
}