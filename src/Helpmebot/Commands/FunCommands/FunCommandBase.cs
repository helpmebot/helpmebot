namespace Helpmebot.Commands.FunCommands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [HelpCategory("Fun")]
    public abstract class FunCommandBase : CommandBase
    {
        protected ISession DatabaseSession { get; private set; }
        protected IMessageService MessageService { get; private set; }

        protected FunCommandBase(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
            IMessageService messageService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.DatabaseSession = databaseSession;
            this.MessageService = messageService;
        }

        protected override void OnPreRun()
        {
            base.OnPreRun();

            var channel = this.DatabaseSession.CreateCriteria<Channel>()
                .Add(Restrictions.Eq("Name", this.CommandSource))
                .UniqueResult<Channel>();

            var hedgehog = false;
            if (channel != null)
            {
                hedgehog = channel.HedgehogMode;
            }

            if (hedgehog)
            {
                throw new CommandErrorException("Sorry, fun commands are currently disabled in this channel.");
            }
        }
    }
}