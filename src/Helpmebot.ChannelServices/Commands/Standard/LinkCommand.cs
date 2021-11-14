namespace Helpmebot.ChannelServices.Commands.Standard
{
    using System.Collections.Generic;
    using System.Linq;
    using NHibernate;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Standard)]
    [CommandInvocation("link")]
    [CommandInvocation("l")]
    public class LinkCommand : CommandBase
    {
        private readonly ILinkerService linkerService;
        private readonly ISession databaseSession;

        public LinkCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ILinkerService linkerService,
            ISession databaseSession) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.linkerService = linkerService;
            this.databaseSession = databaseSession;
        }

        [Help(
            new[] {"", "<link>"},
            new[]
            {
                "Parses a wikilink or MediaWiki page name and returns a URL for each detected wikilink",
                "If no wikilinks are detected in the argument, the entire argument is treated as a link",
                "If no argument is supplied, the last seen link in the channel is used."
            })]
        protected override IEnumerable<CommandResponse> Execute()
        {
            if (this.Arguments.Any())
            {
                var channel = this.databaseSession.GetChannelObject(this.CommandSource);
                var links = this.linkerService.ParseMessageForLinks(string.Join(" ", this.Arguments));

                if (links.Count == 0)
                {
                    links = this.linkerService.ParseMessageForLinks("[[" + string.Join(" ", this.Arguments) + "]]");
                } 
                else if (channel != null && channel.AutoLink) 
                {
                    yield break;
                }

                var message = links.Aggregate(
                    string.Empty,
                    (current, link) =>
                        current + " " + this.linkerService.ConvertWikilinkToUrl(this.CommandSource, link));

                yield return new CommandResponse {Message = message.Trim()};
                yield break;
            }
            yield return new CommandResponse
            {
              Message = this.linkerService.GetLastLinkForChannel(this.CommandSource)
            };
        }
    }
}