namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
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

    [CommandInvocation("rights")]
    [CommandInvocation("userrights")]
    [CommandFlag(Flags.Info)]
    public class UserRightsCommand : CommandBase
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public UserRightsCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
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
            this.apiHelper = apiHelper;
            this.responder = responder;
            this.channelManagementService = channelManagementService;
        }

        [Help("[username]", "Returns the list of groups you or the specified user currently hold")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var username = string.Join(" ", this.Arguments);
            if (this.Arguments.Count == 0)
            {
                username = this.User.Nickname;
            }

            var mediaWikiSite = this.channelManagementService.GetBaseWiki(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            
            try
            {
                string rights;
                try
                {
                    rights = string.Join(", ", mediaWikiApi.GetUserGroups(username).Where(x => x != "*"));
                }
                catch (MissingUserException e)
                {
                    this.Logger.InfoFormat(e, "API reports that user {0} doesn't exist?", username);
                    return this.responder.Respond("commands.command.userrights.missing", this.CommandSource, username);
                }
                catch (MediawikiApiException e)
                {
                    this.Logger.WarnFormat(e, "Encountered error retrieving rights from API for {0}", username);
                    return this.responder.Respond("common.mw-api-error", this.CommandSource);
                }

                if (string.IsNullOrWhiteSpace(rights))
                {
                    return this.responder.Respond("commands.command.userrights.no-rights", this.CommandSource, username);
                }

                return this.responder.Respond(
                    "commands.command.userrights",
                    this.CommandSource,
                    new object[] { username, rights });
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }
        }
    }
}