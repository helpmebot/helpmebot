namespace Helpmebot.Commands.Commands.FunCommands
{
    using System.Collections.Generic;
    using System.Data;
    using Castle.Core.Logging;
    using Helpmebot.Commands.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Protected)]
    [CommandInvocation("curl")]
    public class CurlCommand : CommandBase
    {
        private readonly ISession session;

        public CurlCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession session) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.session = session;
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
                    return new CommandResponse
                    {
                        Message = string.Format("Cannot find configuration for channel {0}", this.CommandSource),
                        IgnoreRedirection = true
                    }.ToEnumerable();
                }

                channel.HedgehogMode = true;
                this.session.SaveOrUpdate(channel);
                txn.Commit();

                return new[]
                {
                    new CommandResponse
                    {
                        Message = "curls up in a ball",
                        ClientToClientProtocol = "ACTION",
                        IgnoreRedirection = true
                    },

                    new CommandResponse
                    {
                        Message = string.Format("All fun commands are now disabled in {0}", this.CommandSource),
                        Destination = CommandResponseDestination.PrivateMessage,
                        IgnoreRedirection = true
                    }
                };
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