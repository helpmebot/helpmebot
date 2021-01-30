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

    [CommandInvocation("pat")]
    [CommandFlag(Flags.Fun)]
    public class PatCommand : TargetedFunCommandBase
    {
        public PatCommand(string commandSource, IUser user, IList<string> arguments, ILogger logger, IFlagService flagService, IConfigurationProvider configurationProvider, IIrcClient client, ISession databaseSession, IMessageService messageService) : base(commandSource, user, arguments, logger, flagService, configurationProvider, client, databaseSession, messageService)
        {
        }

        [Help(new[] {"", "<user>"}, "Pats a user")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            return this.GetResponse("cmdPat");
        }
    }
}