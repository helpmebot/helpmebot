namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Web;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Exceptions;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Exceptions;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("editcount")]
    [CommandInvocation("count")]
    [CommandFlag(Flags.Info)]
    [HelpSummary("Returns your edit count or the edit count for the specified user")]
    public class EditCountCommand : CommandBase
    {
        private readonly IUrlShorteningService urlShorteningService;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public EditCountCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IUrlShorteningService urlShorteningService,
            IMediaWikiApiHelper apiHelper,
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
            this.responder = responder;
            this.channelManagementService = channelManagementService;
        }

        [Help("[username]")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var username = string.Join(" ", this.Arguments);
            if (this.Arguments.Count == 0)
            {
                username = this.User.Nickname;
            }

            int editCount;

            var mediaWikiApi = this.apiHelper.GetApi(this.channelManagementService.GetBaseWiki(this.CommandSource));
            try
            {
                editCount = mediaWikiApi.GetEditCount(username);
            }
            catch (MissingObjectException)
            {
                return this.responder.Respond("commands.common.missing-user", this.CommandSource, username);
            }
            catch (MediawikiApiException e)
            {
                this.Logger.LogWarning(e, "Encountered error retrieving edit count from API for {Username}", username);
                return this.responder.Respond("common.mw-api-error", this.CommandSource);
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }

            var xToolsUrl = this.responder.GetMessagePart(
                "commands.command.editcount.xtools",
                this.CommandSource,
                HttpUtility.UrlEncode(username));

            xToolsUrl = this.urlShorteningService.Shorten(xToolsUrl);

            return this.responder.Respond(
                "commands.command.editcount",
                this.CommandSource,
                new object[]
                {
                    editCount, username, xToolsUrl
                });
        }
    }
}