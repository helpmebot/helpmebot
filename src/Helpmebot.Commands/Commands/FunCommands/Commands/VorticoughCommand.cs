namespace Helpmebot.Commands.Commands.FunCommands.Commands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Commands.Commands.FunCommands;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("vorticough")]
    [CommandInvocation("vortigaunt")]
    [CommandFlag(Flags.Fun)]
    public class VorticoughCommand : FunCommandBase
    {
        public VorticoughCommand(
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
            client,
            databaseSession,
            messageService)
        {
        }

        [Help("", "Produces a random Vortigaunt quote in the current channel.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            yield return new CommandResponse
            {
                Message = this.MessageService.RetrieveMessage(
                    "Vortigaunt",
                    this.CommandSource,
                    new[] {this.User.Nickname})
            };
        }
    }
}