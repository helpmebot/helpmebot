namespace Helpmebot.Commands.Commands.FunCommands.Commands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Commands.Commands.FunCommands;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("sayhi")]
    [CommandFlag(Flags.Fun)]
    public class SayHiCommand : FunCommandBase
    {
        public SayHiCommand(
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

        [Help("", "Bot AI: Says hi to the person who called the command.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            yield return new CommandResponse
            {
                Message = this.MessageService.RetrieveMessage(
                    "CmdSayHi1",
                    this.CommandSource,
                    new[] {this.User.Nickname})
            };
        }
    }
}