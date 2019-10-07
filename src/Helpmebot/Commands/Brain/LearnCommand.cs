namespace Helpmebot.Commands.Brain
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Brain)]
    [CommandInvocation("learn")]
    [CommandInvocation("teach")]
    public class LearnCommand : CommandBase
    {
        private readonly IKeywordService keywordService;

        public LearnCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IKeywordService keywordService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.keywordService = keywordService;
        }

        [RequiredArguments(2)]
        [Help(new[]{"<keyword> <message>", "@action <keyword> <message>"}, 
            new[]{"Creates a new learnt command invoked by <keyword> to respond with the provided message",
                "Optionally sends message as a CTCP ACTION (aka a /me command) if @action is provided before the first parameter."})]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var action = false;
            var args = new List<string>(this.Arguments);

            if (args.Count >= 3)
            {
                if (args[0] == "@action")
                {
                    action = true;
                    args.PopFromFront();
                }
            }

            var keywordName = args.PopFromFront();
            this.keywordService.Create(keywordName, string.Join(" ", Enumerable.ToArray(args)), action);
  
            yield return new CommandResponse
            {
                Message = "New command created",
                Type = CommandResponseType.Notice,
                Destination = CommandResponseDestination.PrivateMessage
            };
        }
    }
}