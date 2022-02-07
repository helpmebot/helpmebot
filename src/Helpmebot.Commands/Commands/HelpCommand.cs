namespace Helpmebot.Commands.Commands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Standard)]
    public class HelpCommand : Stwalkerster.Bot.CommandLib.Commands.HelpCommand
    {
        private readonly IResponder responder;

        public HelpCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            ICommandParser commandParser,
            IIrcClient client,
            IResponder responder)
            : base(commandSource, user, arguments, logger, flagService, configurationProvider, commandParser, client)
        {
            this.responder = responder;
        }

        // Not redundant - needed by the reflection-based command parser.
        // ReSharper disable once RedundantOverriddenMember
        protected override IEnumerable<CommandResponse> Execute()
        {
            return base.Execute();
        }

        protected override IEnumerable<CommandResponse> OnNoArguments()
        {
            return this.responder.Respond("commands.command.help", this.CommandSource);
        }
    }
}