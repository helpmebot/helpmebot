namespace Helpmebot.Commands.BotManagement
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flag.Owner)]
    // ReSharper disable once StringLiteralTypo
    [CommandInvocation("synchelp")]
    public class SyncHelpCommand : CommandBase
    {
        private readonly IHelpSyncService helpSyncService;

        public SyncHelpCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IHelpSyncService helpSyncService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.helpSyncService = helpSyncService;
        }

        [Help("", "Synchronises the help pages on the documentation wiki")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            this.helpSyncService.DoSync(this.User);
            yield return new CommandResponse
            {
                Message = "Sync complete."
            };
        }
    }
}