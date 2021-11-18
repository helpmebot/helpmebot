namespace Helpmebot.Commands.Commands.FunCommands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [HelpCategory("Fun")]
    public abstract class FunCommandBase : CommandBase
    {
        public IResponder Responder { get; }
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
            IMessageService messageService,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.Responder = responder;
            this.DatabaseSession = databaseSession;
            this.MessageService = messageService;
        }

        protected override IEnumerable<CommandResponse> OnPreRun(out bool abort)
        {
            var channel = this.DatabaseSession.GetChannelObject(this.CommandSource);

            abort = false;
            if (channel != null)
            {
                abort = channel.HedgehogMode;
            }
            
            if (abort)
            {
                return this.Responder.Respond("funcommands.disabled", this.CommandSource, channel?.Name);
            }

            return null;
        }
    }
}