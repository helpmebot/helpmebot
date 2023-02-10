namespace Helpmebot.Commands.Commands.BotInfo
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("ping")]
    [CommandFlag(Flags.Standard)]
    [HelpSummary("Replies to a ping with a pong")]
    public class PingCommand : CommandBase
    {
        private readonly IResponder responder;

        public PingCommand(
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

        [Help("[username]")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            string name;

            if (this.Arguments.Count == 0)
            {
                name = this.User.Nickname;
                return this.responder.Respond("commands.command.ping", this.CommandSource, name);
            }
            
            name = string.Join(" ", this.Arguments);
            return this.responder.Respond("commands.command.ping.user", this.CommandSource, name);
        }
    }
}