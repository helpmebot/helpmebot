namespace Helpmebot.Commands.WikiInformation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Castle.Core.Logging;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("userinfo")]
    [CommandFlag(Flags.Info)]
    public class UserInfoCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly ILinkerService linkerService;
        private readonly IUrlShorteningService urlShortener;
        private readonly IMediaWikiApiHelper apiHelper;

        public UserInfoCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
            ILinkerService linkerService,
            IUrlShorteningService urlShortener,
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
            this.linkerService = linkerService;
            this.urlShortener = urlShortener;
            this.apiHelper = apiHelper;
        }

        [Help("[username]", "Gives a batch of information on the specified user.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiSiteObject = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSiteObject);

            var username = string.Join(" ", this.Arguments);
            if (this.Arguments.Count == 0)
            {
                username = this.User.Nickname;
            }

            string message;
            try
            {
                var editCount = mediaWikiApi.GetEditCount(username);
                var userGroups = string.Join(", ", mediaWikiApi.GetUserGroups(username).Where(x => x != "*"));
                var registrationDate = mediaWikiApi.GetRegistrationDate(username);

                int ageYears;
                TimeSpan ageSpan;
                registrationDate.Value.CalculateDuration(out ageYears, out ageSpan);

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

                var userBlockLogBuilder = new UriBuilder(
                    this.linkerService.ConvertWikilinkToUrl(this.CommandSource, "Special:Log"));
                var queryParts = HttpUtility.ParseQueryString(userBlockLogBuilder.Query);
                queryParts["type"] = "block";
                queryParts["page"] = string.Format("User:{0}", username);
                userBlockLogBuilder.Query = queryParts.ToString();
                var userBlockLog = userBlockLogBuilder.Uri.ToString();

                var editRate = editCount / (DateTime.Now - registrationDate.Value).TotalDays;
                var isBlocked = mediaWikiSiteObject.GetBlockInformation(username).Any();

                message = string.Format(
                    "User: {0} | Talk: {1} | Contribs: {2} | BlockLog: {3} | CA: {11} | Groups: {4} | Age: {5}y {10:d\\d\\ h\\h\\ m\\m} | Reg: {6:u} | Count: {8} | Activity: {7:#####.###} {9}",
                    this.urlShortener.Shorten(userPage),
                    this.urlShortener.Shorten(userTalk),
                    this.urlShortener.Shorten(userContributions),
                    this.urlShortener.Shorten(userBlockLog),
                    userGroups,
                    ageYears,
                    registrationDate.Value,
                    editRate,
                    editCount,
                    isBlocked ? "| BLOCKED" : string.Empty,
                    ageSpan,
                    this.urlShortener.Shorten(centralAuth));
            }
            catch (MediawikiApiException ex)
            {
                this.Logger.WarnFormat(ex, "Error retrieving user info from API for user {0}", username);
                return new[] {new CommandResponse {Message = "Encountered error retrieving result from API"}};
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }

            return new[] {new CommandResponse {Message = message}};
        }
    }
}