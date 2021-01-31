namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Web;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("editcount")]
    [CommandInvocation("count")]
    [CommandFlag(Flags.Info)]
    public class EditCountCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IUrlShorteningService urlShorteningService;
        private readonly IMediaWikiApiHelper apiHelper;

        public EditCountCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
            IUrlShorteningService urlShorteningService,
            IMediaWikiApiHelper apiHelper) : base(
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
        }

        [Help("[username]", "Returns your edit count or the edit count for the specified user")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var username = string.Join(" ", this.Arguments);
            if (this.Arguments.Count == 0)
            {
                username = this.User.Nickname;
            }

            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);

            int editCount;
            try
            {
                editCount = mediaWikiApi.GetEditCount(username);
            }
            catch (MediawikiApiException e)
            {
                this.Logger.WarnFormat(e, "Encountered error retrieving edit count from API for {0}", username);
                return new[] {new CommandResponse {Message = "Encountered error retrieving result from API"}};
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }
            
            var xToolsUrl = string.Format(
                "https://tools.wmflabs.org/xtools-ec/index.php?user={0}&project=en.wikipedia.org",
                HttpUtility.UrlEncode(username));

            xToolsUrl = this.urlShorteningService.Shorten(xToolsUrl);

            var message = "The edit count of [[User:{1}]] is {0}  (For more detailed information, see {2} )";

            return new[]
            {
                new CommandResponse
                {
                    Message = string.Format(message, editCount, username, xToolsUrl)
                }
            };
        }
    }
}