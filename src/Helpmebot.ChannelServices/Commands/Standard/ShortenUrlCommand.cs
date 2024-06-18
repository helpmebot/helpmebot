namespace Helpmebot.ChannelServices.Commands.Standard
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("shorten")]
    [CommandInvocation("shorturl")]
    [CommandInvocation("isgd")]
    [CommandInvocation("hmbim")]
    [CommandFlag(Flags.Standard)]
    [HelpSummary("Shortens the provided URL")]
    public class ShortenUrlCommand : CommandBase
    {
        private readonly IUrlShorteningService urlShorteningService;

        public ShortenUrlCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IUrlShorteningService urlShorteningService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.urlShorteningService = urlShorteningService;
        }

        [Help("<url>")]
        [RequiredArguments(1)]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var shortUrls = this.Arguments.Select(this.urlShorteningService.Shorten);
            yield return new CommandResponse {Message = string.Join(" ", shortUrls)};
        }
    }
}