namespace Helpmebot.Brain.Commands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Brain.Services.Interfaces;
    using Helpmebot.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Brain)]
    [CommandInvocation("forget")]
    public class ForgetCommand : CommandBase
    {
        private readonly IKeywordService keywordService;

        public ForgetCommand(
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

        [RequiredArguments(1)]
        [Help("<keyword> [keyword...]", "Removes the provided keywords from the learnt command list.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            foreach (var argument in this.Arguments)
            {
                this.keywordService.Delete(argument);
            }
            
            yield return new CommandResponse
            {
                Message = "Command removed",
                Type = CommandResponseType.Notice,
                Destination = CommandResponseDestination.PrivateMessage
            };
        }
    }
}