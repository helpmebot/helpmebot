namespace Helpmebot.Commands.Commands.FunCommands
{
    using System.Collections.Generic;
    using System.Data;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
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

    [CommandFlag(Flags.Protected)]
    [CommandInvocation("curl")]
    public class CurlCommand : CommandBase
    {
        private readonly ISession session;
        private readonly IResponder responder;

        public CurlCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession session,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.session = session;
            this.responder = responder;
        }

        [Help("", "Disables all fun commands in the current channel.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var txn = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var channel = this.session.CreateCriteria<Channel>()
                    .Add(Restrictions.Eq("Name", this.CommandSource))
                    .UniqueResult<Channel>();

                if (channel == null)
                {
                    throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
                }

                channel.HedgehogMode = true;
                this.session.SaveOrUpdate(channel);
                txn.Commit();

                return this.responder.Respond("funcommands.command.curl", this.CommandSource, channel);
            }
            finally
            {
                if (!txn.WasCommitted)
                {
                    txn.Rollback();
                }
            }
        }
    }
}