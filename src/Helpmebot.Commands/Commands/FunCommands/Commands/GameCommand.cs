namespace Helpmebot.Commands.Commands.FunCommands.Commands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Commands.Commands.FunCommands;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("game")]
    [CommandFlag(Flags.Fun)]
    public class GameCommand : FunCommandBase
    {
        public GameCommand(
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
            client,
            databaseSession,
            responder)
        {
        }

        [Help("", "Plays a game")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            return this.Responder.Respond("funcommands.command.the-game", this.CommandSource, this.User.Nickname);
        }
    }
}