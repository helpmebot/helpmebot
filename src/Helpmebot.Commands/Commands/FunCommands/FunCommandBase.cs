namespace Helpmebot.Commands.Commands.FunCommands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
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
                return new[]
                {
                    new CommandResponse
                    {
                        Message = "Sorry, fun commands are currently disabled in this channel.",
                        Destination = CommandResponseDestination.PrivateMessage
                    }
                };
            }

            return null;
        }
    }
}