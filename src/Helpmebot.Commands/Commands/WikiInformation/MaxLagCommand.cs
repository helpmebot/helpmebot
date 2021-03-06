namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("maxlag")]
    [CommandFlag(Flags.Info)]
    public class MaxLagCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IMediaWikiApiHelper apiHelper;

        public MaxLagCommand(
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

        [Help("", "Returns the maximum replication lag on the channel's MediaWiki instance")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiSiteObject = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSiteObject);

            var message =
                "The maximum replication lag is {0} second(s). (Parts of the wiki may appear to be {0} second(s) out-of-date).";

            yield return new CommandResponse {Message = string.Format(message, mediaWikiApi.GetMaxLag())};

            this.apiHelper.Release(mediaWikiApi);
        }
    }
}