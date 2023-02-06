namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using Castle.Core.Logging;
    using Helpmebot.Commands.ExtensionMethods;
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

    [CommandInvocation("userinfo")]
    [CommandFlag(Flags.Info)]
    public class UserInfoCommand : CommandBase
    {
        private readonly ILinkerService linkerService;
        private readonly IUrlShorteningService urlShortener;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public UserInfoCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ILinkerService linkerService,
            IUrlShorteningService urlShortener,
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
            this.linkerService = linkerService;
            this.urlShortener = urlShortener;
            this.apiHelper = apiHelper;
            this.responder = responder;
            this.channelManagementService = channelManagementService;
        }

        [Help("[username]", "Gives a batch of information on the specified user.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiApi = this.apiHelper.GetApi(this.channelManagementService.GetBaseWiki(this.CommandSource));

            var username = string.Join(" ", this.Arguments);
            if (this.Arguments.Count == 0)
            {
                username = this.User.Nickname;
            }
            
            try
            {
                var userPage = this.linkerService.ConvertWikilinkToUrl(
                    this.CommandSource,
                    string.Format("User:{0}", username));
                var userTalk = this.linkerService.ConvertWikilinkToUrl(
                    this.CommandSource,
                    string.Format("User_talk:{0}", username));
                var userContributions = this.linkerService.ConvertWikilinkToUrl(
                    this.CommandSource,
                    string.Format("Special:Contributions/{0}", username));
                var centralAuth = this.linkerService.ConvertWikilinkToUrl(
                    this.CommandSource,
                    string.Format("meta:Special:CentralAuth/{0}", username));

                var editCount = 0;
                var userGroups = string.Empty;
                DateTime? registrationDate = DateTime.MinValue;
                var ageYears = 0;
                TimeSpan ageSpan = TimeSpan.Zero;
                var editRate = 0d;

                var isIp = true;

                // IP addresses aren't valid for username lookups
                IPAddress ip;
                if (!IPAddress.TryParse(username, out ip))
                {
                    isIp = false;
                    editCount = mediaWikiApi.GetEditCount(username);
                    userGroups = string.Join(", ", mediaWikiApi.GetUserGroups(username).Where(x => x != "*"));
                    registrationDate = mediaWikiApi.GetRegistrationDate(username);
                    registrationDate.Value.CalculateDuration(out ageYears, out ageSpan);
                    editRate = editCount / (DateTime.Now - registrationDate.Value).TotalDays;
                }

                var userBlockLogBuilder = new UriBuilder(
                    this.linkerService.ConvertWikilinkToUrl(this.CommandSource, "Special:Log"));
                var queryParts = HttpUtility.ParseQueryString(userBlockLogBuilder.Query);
                queryParts["type"] = "block";
                queryParts["page"] = string.Format("User:{0}", username);
                userBlockLogBuilder.Query = queryParts.ToString();
                var userBlockLog = userBlockLogBuilder.Uri.ToString();

                var isBlocked = mediaWikiApi.GetBlockInformation(username).Any();

                var format = "commands.command.userinfo.user";
                if (isIp)
                {
                    format = "commands.command.userinfo.ip";
                }

                var blocked = this.responder.GetMessagePart("commands.command.userinfo.blocked", this.CommandSource);
                var parameters = new object[]
                {
                    this.urlShortener.Shorten(userPage),
                    this.urlShortener.Shorten(userTalk),
                    this.urlShortener.Shorten(userContributions),
                    this.urlShortener.Shorten(userBlockLog),
                    userGroups,
                    ageYears,
                    registrationDate.Value,
                    editRate,
                    editCount,
                    isBlocked ? blocked : string.Empty,
                    ageSpan,
                    this.urlShortener.Shorten(centralAuth)
                };
                
                return this.responder.Respond(format, this.CommandSource, parameters);
            }
            catch (GeneralMediaWikiApiException ex) 
            {
                if (ex.Message == "Missing user")
                {
                    return this.responder.Respond("commands.command.userinfo.missing", this.CommandSource, username);
                }

                throw;
            }
            catch (MediawikiApiException ex)
            {
                this.Logger.WarnFormat(ex, "Error retrieving user info from API for user {0}", username);
                return this.responder.Respond("common.mw-api-error", this.CommandSource);
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }
        }
    }
}