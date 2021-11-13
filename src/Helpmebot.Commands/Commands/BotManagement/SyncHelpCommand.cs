namespace Helpmebot.Commands.Commands.BotManagement
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Owner)]
    // ReSharper disable once StringLiteralTypo
    [CommandInvocation("synchelp")]
    public class SyncHelpCommand : CommandBase
    {
        private readonly IHelpSyncService helpSyncService;
        private readonly ISession databaseSession;

        public SyncHelpCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IHelpSyncService helpSyncService,
            ISession databaseSession) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.helpSyncService = helpSyncService;
            this.databaseSession = databaseSession;
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