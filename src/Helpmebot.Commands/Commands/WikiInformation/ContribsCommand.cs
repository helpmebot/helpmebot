namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Exceptions;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("contribs")]
    [CommandFlag(Flags.Info)]
    public class ContribsCommand : CommandBase
    {
        private readonly IUrlShorteningService urlShorteningService;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly ILinkerService linkerService;
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public ContribsCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IUrlShorteningService urlShorteningService,
            IMediaWikiApiHelper apiHelper,
            ILinkerService linkerService,
            IResponder responder,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.urlShorteningService = urlShorteningService;
            this.apiHelper = apiHelper;
            this.linkerService = linkerService;
            this.responder = responder;
            this.channelManagementService = channelManagementService;
        }

        [RequiredArguments(1)]
        [Help("<username>", "Returns information on the last contribution for this user")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiSite = this.channelManagementService.GetBaseWiki(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);

            var user = this.OriginalArguments;
            var contribsLink = this.linkerService.ConvertWikilinkToUrl(this.CommandSource, "Special:Contribs/" + user);

            var exists = true;
            try
            {
                mediaWikiApi.GetRegistrationDate(user);
            }
            catch (MissingUserException)
            {
                exists = false;
            }
            
            var contribs = mediaWikiApi.GetContributions(user, 1).ToList();
            
            var lastContrib = "";
            if (contribs.Any())
            {
                var last = contribs.First();

                lastContrib = this.responder.GetMessagePart(
                    "commands.command.contribs.last",
                    this.CommandSource,
                    new object[]
                    {
                        last.Title,
                        last.Timestamp,
                        last.Comment,
                        this.urlShorteningService.Shorten(
                            this.linkerService.ConvertWikilinkToUrl(this.CommandSource, "Special:Diff/" + last.RevId))
                    });
            }

            if (!exists)
            {
                lastContrib = this.responder.GetMessagePart("commands.command.contribs.missing", this.CommandSource);
            }

            return this.responder.Respond("commands.command.contribs", this.CommandSource, new object[]
            {
                user,
                this.urlShorteningService.Shorten(contribsLink),
                lastContrib
            });
        }
    }
}
