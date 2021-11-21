namespace Helpmebot.ChannelServices.Commands.Configuration
{
    using System.Collections.Generic;
    using System.Data;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.LocalConfiguration)]
    [CommandFlag(Flags.Configuration, true)]
    [CommandInvocation("silence")]
    public class SilenceCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IResponder responder;

        public SilenceCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
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
            this.databaseSession = databaseSession;
            this.responder = responder;
        }

        [SubcommandInvocation("enable")]
        [Help("", "Enables silent mode for the current channel")]
        protected IEnumerable<CommandResponse> EnableCommand()
        {
            using (var txn = this.databaseSession.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var channel = this.databaseSession.GetChannelObject(this.CommandSource);

                if (channel == null)
                {
                    throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
                }

                channel.Silenced = true;
                this.databaseSession.Save(channel);
                txn.Commit();

                return this.responder.Respond("channelservices.command.silence.enabled", this.CommandSource);
            }
        }

        [SubcommandInvocation("disable")]
        [Help("", "Disables silent mode for the current channel")]
        protected IEnumerable<CommandResponse> DisableCommand()
        {
            using (var txn = this.databaseSession.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var channel = this.databaseSession.GetChannelObject(this.CommandSource);

                if (channel == null)
                {
                    throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
                }

                channel.Silenced = false;
                this.databaseSession.Save(channel);
                txn.Commit();

                return this.responder.Respond("channelservices.command.silence.disabled", this.CommandSource);
            }
        }
    }
}