namespace Helpmebot.Commands.Brain
{
    using System.Collections.Generic;
    using System.Globalization;
    using Castle.Core.Logging;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flag.Standard)]
    public class BrainRetrievalCommand : CommandBase
    {
        private readonly IKeywordService keywordService;

        public BrainRetrievalCommand(
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

        protected override IEnumerable<CommandResponse> Execute()
        {
            var keyword = this.keywordService.Get(this.InvokedAs);
            
            IDictionary<string, object> dict = new Dictionary<string, object>();

            dict.Add("username", this.User.Username);
            dict.Add("nickname", this.User.Nickname);
            dict.Add("hostname", this.User.Hostname);
            dict.Add("channel", this.CommandSource);

            for (var i = 0; i < this.Arguments.Count; i++)
            {
                dict.Add(i.ToString(CultureInfo.InvariantCulture), this.Arguments[i]);
                dict.Add(i + "*", string.Join(" ", this.Arguments, i, this.Arguments.Count - i));
            }

            var wordResponse = keyword.Response.FormatWith(dict);

            if (keyword.Action)
            {
                yield return new CommandResponse
                {
                    ClientToClientProtocol = "ACTION",
                    Message = wordResponse,
                    IgnoreRedirection = true
                };
            }
            else
            {
                yield return new CommandResponse
                {
                    Message = wordResponse
                };
            }
        }
    }
}