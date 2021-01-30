namespace Helpmebot.ChannelServices.Commands.CrossChannel
{
    using System.Collections.Generic;
    using System.Data;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flag.Standard)]
    public class CrossChannelNotifyCommand : CommandBase
    {
        private readonly ICrossChannelService crossChannelService;
        private readonly ISession databaseSession;

        public CrossChannelNotifyCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ICrossChannelService crossChannelService,
            ISession databaseSession) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.crossChannelService = crossChannelService;
            this.databaseSession = databaseSession;
        }

        protected override IEnumerable<CommandResponse> Execute()
        {
            this.databaseSession.BeginTransaction(IsolationLevel.RepeatableRead);

            var channel = this.databaseSession.GetChannelObject(this.CommandSource);

            this.crossChannelService.Notify(
                channel,
                this.OriginalArguments,
                this.databaseSession,
                this.Client,
                this.User);
            
            this.databaseSession.Transaction.Commit();
            
            yield break;
        }
    }
}