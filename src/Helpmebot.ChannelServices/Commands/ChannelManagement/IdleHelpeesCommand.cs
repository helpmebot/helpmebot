namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Owner)]
    [CommandInvocation("idlehelpees")]
    public class IdleHelpeesCommand :CommandBase
    {
        private readonly IHelpeeManagementService helpeeManagementService;

        public IdleHelpeesCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IHelpeeManagementService helpeeManagementService)
            : base(commandSource, user, arguments, logger, flagService, configurationProvider, client)
        {
            this.helpeeManagementService = helpeeManagementService;
        }

        protected override IEnumerable<CommandResponse> Execute()
        {
            var helpees = this.helpeeManagementService.Helpees
                .Select(
                    delegate(KeyValuePair<IrcUser, DateTime> x)
                    {
                        if (x.Value == DateTime.MinValue)
                        {
                            return $"{x.Key.Nickname} (idle since bot startup)";
                        }
                        return $"{x.Key.Nickname} (idle {DateTime.UtcNow - x.Value:d\\d\\ hh\\:mm\\:ss})";
                    })
                .Aggregate(string.Empty, (cur, next) => cur + "; " + next);

            var lastActiveHelpers = this.helpeeManagementService.Helpers
                .OrderByDescending(x => x.Value)
                .Where(x => x.Value != DateTime.MinValue)
                .Take(3)
                .Select(x => $"{x.Key.Nickname.Insert(1, "_")} (idle {DateTime.UtcNow - x.Value:d\\d\\ hh\\:mm\\:ss})")
                .Aggregate(string.Empty, (cur, next) => cur + "; " + next);

            helpees = helpees.TrimStart(' ', ';');
            lastActiveHelpers = lastActiveHelpers.TrimStart(' ', ';');
            
            yield return new CommandResponse
                {Message = $"Helpees: {helpees} | Last 3 active helpers: {lastActiveHelpers}"};
        }
    }
}