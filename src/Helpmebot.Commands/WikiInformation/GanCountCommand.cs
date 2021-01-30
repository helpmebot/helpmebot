namespace Helpmebot.Commands.WikiInformation
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
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

    [CommandFlag(Flags.Info)]
    [CommandInvocation("gancount")]
    public class GanCountCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IMediaWikiApiHelper apiHelper;

        public GanCountCommand(
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

        [Help("", "Returns the number of GAN submissions awaiting review")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var categoryName = "Good article nominees awaiting review";
            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            var categorySize = mediaWikiApi.GetCategorySize(categoryName);
            this.apiHelper.Release(mediaWikiApi);
            
            return new[]
            {
                new CommandResponse
                {
                    Message = string.Format("[[Category:{0}]] has {1} items", categoryName, categorySize)
                }
            };
        }
    }
}