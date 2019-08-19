namespace Helpmebot.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Services;
    using Helpmebot.Services.Interfaces;
    using Newtonsoft.Json;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Info)]
    [CommandInvocation("afccount")]
    [CommandInvocation("afcbacklog")]
    public class AfcCountCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly DraftStatusService draftStatusService;

        public AfcCountCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
            IMediaWikiApiHelper apiHelper,
            DraftStatusService draftStatusService) : base(
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
            this.draftStatusService = draftStatusService;
        }

        [Help("", "Returns the number of AfC submissions awaiting review")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(string.Empty);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            
            var categorySize = this.draftStatusService.GetPendingDraftCount(mediaWikiApi);

            string ts;
            var pageContent = mediaWikiApi.GetPageContent("User:Stwalkerster/hmb-afc-backlog.json", out ts);
            
            this.apiHelper.Release(mediaWikiApi);
            
            var mapping = JsonConvert.DeserializeObject<Dictionary<int, string>>(pageContent);
            var item = mapping.Where(x => categorySize >= x.Key).Max(x => x.Key);

            return new[]
            {
                new CommandResponse
                {
                    Message = string.Format(
                        "There are {1} drafts pending review - {2}",
                        categorySize,
                        mapping[item])
                }
            };
        }
    }
}