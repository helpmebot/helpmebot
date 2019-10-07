namespace Helpmebot.Commands.FunCommands.Commands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Model;
    using NHibernate;
    using Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("electrocute")]
    [CommandFlag(Flags.Fun)]
    public class ElectrocuteCommand : ProtectedTargetedFunCommandBase
    {
        public ElectrocuteCommand(string commandSource, IUser user, IList<string> arguments, ILogger logger, IFlagService flagService, IConfigurationProvider configurationProvider, IIrcClient client, ISession databaseSession, IMessageService messageService) : base(commandSource, user, arguments, logger, flagService, configurationProvider, client, databaseSession, messageService)
        {
        }

        [Help(new[] {"", "<user>"}, "High voltages tend to be dangerous...")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            return this.GetResponse("CmdElectrocute");
        }
    }
}