namespace Helpmebot.Commands.CategoryMonitoring
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Info)]
    public class ForceUpdateCommand : CommandBase
    {
        private readonly ICategoryWatcherBackgroundService categoryWatcherService;
        private readonly ISession databaseSession;

        public ForceUpdateCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ICategoryWatcherBackgroundService categoryWatcherService,
            ISession databaseSession) : base(
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
        }

        protected override IEnumerable<CommandResponse> Execute()
        {
            var channel = this.databaseSession.CreateCriteria<Channel>()
                .Add(Restrictions.Eq("Name", this.CommandSource))
                .UniqueResult<Channel>();

            if (channel == null)
            {
                throw new CommandErrorException("Could not retrieve channel configuration.");
            }
            
            this.categoryWatcherService.ForceUpdate(this.InvokedAs, channel);
            
            return null;
        }
    }
}