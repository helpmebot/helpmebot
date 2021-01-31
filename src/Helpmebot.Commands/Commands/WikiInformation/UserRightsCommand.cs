namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Exceptions;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("rights")]
    [CommandFlag(Flags.Info)]
    public class UserRightsCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IMediaWikiApiHelper apiHelper;

        public UserRightsCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
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
            this.apiHelper = apiHelper;
        }

        [Help("[username]", "Returns the list of groups you or the specified user currently hold")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var username = string.Join(" ", this.Arguments);
            if (this.Arguments.Count == 0)
            {
                username = this.User.Nickname;
            }

            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            
            try
            {
                string rights;
                try
                {
                    rights = string.Join(", ", mediaWikiApi.GetUserGroups(username).Where(x => x != "*"));
                }
                catch (MediawikiApiException e)
                {
                    this.Logger.WarnFormat(e, "Encountered error retrieving rights from API for {0}", username);
                    return new[] {new CommandResponse {Message = "Encountered error retrieving result from API"}};
                }

                if (string.IsNullOrWhiteSpace(rights))
                {
                    return new[]
                    {
                        new CommandResponse {Message = string.Format("[[User:{0}]] has no rights assigned.", username)}
                    };
                }

                return new[]
                {
                    new CommandResponse
                    {
                        Message = string.Format("[[User:{0}]] has the following rights: {1}", username, rights)
                    }
                };
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }
        }
    }
}