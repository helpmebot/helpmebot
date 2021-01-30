namespace Helpmebot.Commands.FunCommands.Commands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("whistle")]
    [CommandFlag(Flags.Fun)]
    public class WhistleCommand : FunCommandBase
    {
        public WhistleCommand(
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

        [Help("", "Makes the bot whistle a tune")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            yield return new CommandResponse
            {
                Message = this.MessageService.RetrieveMessage(
                    "CmdWhistle",
                    this.CommandSource,
                    new[] {this.User.Nickname})
            };
        }
    }
}