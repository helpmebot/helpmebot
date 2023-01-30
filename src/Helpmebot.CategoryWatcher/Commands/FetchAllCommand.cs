namespace Helpmebot.CategoryWatcher.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CategoryWatcher.Services.Interfaces;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Mono.Options;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("fetchall")]
    [CommandFlag(Flags.Protected)]
    public class FetchAllCommand : CommandBase
    {
        private readonly ICategoryWatcherBackgroundService categoryWatcherService;
        private readonly ISession databaseSession;
        private readonly IResponder responder;

        public FetchAllCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ICategoryWatcherBackgroundService categoryWatcherService,
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
            this.categoryWatcherService = categoryWatcherService;
            this.databaseSession = databaseSession;
            this.responder = responder;
        }

        [Help(
            "[--all]",
            new[]
            {
                "Returns the current state of all category watchers configured in the current channel.",
                "Use --all to return the current state of all category watchers configured on the bot."
            })]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var allKeywords = false;
            var opts = new OptionSet
            {
                {"all", x => allKeywords = true}
            };
            opts.Parse(this.Arguments);

            var channelObject = this.databaseSession.GetChannelObject(this.CommandSource);
            if (channelObject == null && !allKeywords)
            {
                return this.responder.Respond("catwatcher.command.fetchall.must-run-in-channel", this.CommandSource);
            }

            List<string> validKeywords;
            if (allKeywords)
            {
                validKeywords = this.categoryWatcherService.GetValidWatcherKeys().ToList();
            }
            else
            {
                validKeywords = this.categoryWatcherService.GetWatchedCategories(channelObject).ToList();
            }

            foreach (var flag in validKeywords)
            {
                this.categoryWatcherService.ForceUpdate(flag, channelObject);
            }

            return Array.Empty<CommandResponse>();
        }
    }
}