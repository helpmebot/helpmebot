namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Exceptions;
    using Stwalkerster.Bot.MediaWikiLib.Model;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("contribs")]
    [CommandFlag("I")]
    public class ContribsCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IUrlShorteningService urlShorteningService;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly ILinkerService linkerService;

        public ContribsCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
            IUrlShorteningService urlShorteningService,
            IMediaWikiApiHelper apiHelper,
            ILinkerService linkerService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.databaseSession = databaseSession;
            this.urlShorteningService = urlShorteningService;
            this.apiHelper = apiHelper;
            this.linkerService = linkerService;
        }

        [RequiredArguments(1)]
        [Help("<username>", "Returns information on the last contribution for this user")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
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
            
            List<Contribution> contribs = mediaWikiApi.GetContributions(user, 1).ToList();
            
            var lastContrib = "";
            if (contribs.Any())
            {
                var last = contribs.First();
                lastContrib = string.Format(
                    " The last contribution was to [[{0}]] at {1} ( {3} ), with the comment: {2}",
                    last.Title,
                    last.Timestamp,
                    last.Comment,
                    this.urlShorteningService.Shorten(
                        this.linkerService.ConvertWikilinkToUrl(this.CommandSource, "Special:Diff/" + last.RevId)));
            }

            if (!exists)
            {
                lastContrib = " However, this user does not appear to exist on the local wiki.";
            }
            
            var messageBase = "The full list of contributions for [[User:{0}]] can be found at {1} .{2}";
            
            return new[]
            {
                new CommandResponse
                {
                    Message = String.Format(
                        messageBase,
                        user,
                        this.urlShorteningService.Shorten(contribsLink),
                        lastContrib)
                }
            };
        }
    }
}
