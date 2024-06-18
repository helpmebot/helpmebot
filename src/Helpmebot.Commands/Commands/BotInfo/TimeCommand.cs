namespace Helpmebot.Commands.Commands.BotInfo
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("time")]
    [CommandInvocation("date")]
    [HelpSummary("Returns the current UTC date and time")]
    [CommandFlag(Flags.Standard)]
    public class TimeCommand : CommandBase
    {
        private readonly IResponder responder;

        public TimeCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
        }

        protected override IEnumerable<CommandResponse> Execute() => 
            this.responder.Respond("commands.command.time", this.CommandSource, DateTime.UtcNow);
    }
}