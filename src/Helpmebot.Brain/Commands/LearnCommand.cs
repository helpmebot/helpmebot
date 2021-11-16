namespace Helpmebot.Brain.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Attributes;
    using Helpmebot.Brain.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Brain)]
    [CommandInvocation("learn")]
    [CommandInvocation("teach")]
    [HelpCategory("Brain")]
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
        [Help(new[]{"<keyword> <message>", "--action <keyword> <message>"}, 
            new[]{"Creates a new learnt command invoked by <keyword> to respond with the provided message",
                "Optionally sends message as a CTCP ACTION (aka a /me command) if --action is provided before the first parameter.",
                "@action is accepted in place of --action for backwards compatibility."
            })]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var action = false;
            var args = new List<string>(this.Arguments);

            if (args.Count >= 3)
            {
                if (args[0] == "@action" || args[0] == "--action")
                {
                    action = true;
                    args.PopFromFront();
                }
            }

            var keywordName = args.PopFromFront();
            this.keywordService.Create(keywordName, string.Join(" ", Enumerable.ToArray(args)), action);
            return this.responder.Respond("brain.command.learn", this.CommandSource, keywordName);
        }
    }
}