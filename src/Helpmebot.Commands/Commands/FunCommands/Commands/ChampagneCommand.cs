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

    [CommandInvocation("champagne")]
    [CommandFlag(Flags.Fun)]
    public class ChampagneCommand : TargetedFunCommandBase
    {
        public ChampagneCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
            IResponder responder) : base(commandSource, user, arguments, logger, flagService, configurationProvider, client, databaseSession, responder)
        {
        }

        [Help(new[] {"", "<user>"}, "Gives a user a more classy drink.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            return this.GetResponse("funcommands.command.champagne");
        }
    }
}