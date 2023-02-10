namespace Helpmebot.Brain.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.Attributes;
    using Helpmebot.Brain.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Brain)]
    [CommandInvocation("learn")]
    [CommandInvocation("teach")]
    [HelpCategory("Brain")]
    [HelpSummary("Creates a new learnt command")]
    public class LearnCommand : CommandBase
    {
        private readonly IKeywordService keywordService;
        private readonly IResponder responder;

        public LearnCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IKeywordService keywordService,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.keywordService = keywordService;
            this.responder = responder;
        }

        [RequiredArguments(2)]
        [Help("<keyword> <message>", 
            "Creates a new learnt command invoked by <keyword> to respond with the provided message")]
        [CommandParameter("action", "Send the response as a CTCP action (aka a /me command) instead of a normal message", "action", typeof(bool))]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var action = this.Parameters.GetParameter("action", false);
            var args = new List<string>(this.Arguments);

            var keywordName = args.PopFromFront();
            this.keywordService.Create(keywordName, string.Join(" ", Enumerable.ToArray(args)), action);
            return this.responder.Respond("brain.command.learn", this.CommandSource, keywordName);
        }
    }
}