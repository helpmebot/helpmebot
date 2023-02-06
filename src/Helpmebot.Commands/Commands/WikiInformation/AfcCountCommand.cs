namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using CoreServices.Services;
    using Helpmebot.Commands.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Newtonsoft.Json;
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
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IDraftStatusService draftStatusService;
        private readonly IResponder responder;

        public AfcCountCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMediaWikiApiHelper apiHelper,
            IDraftStatusService draftStatusService,
            IResponder responder
            ) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.apiHelper = apiHelper;
            this.draftStatusService = draftStatusService;
            this.responder = responder;
        }

        [Help("", "Returns the number of AfC submissions awaiting review")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiApi = this.apiHelper.GetApi(MediaWikiApiHelper.WikipediaEnglish, false);
            
            var categorySize = this.draftStatusService.GetPendingDraftCount(mediaWikiApi);

            var pageContent = mediaWikiApi.GetPageContent("User:Stwalkerster/hmb-afc-backlog.json", out _);
            
            this.apiHelper.Release(mediaWikiApi);
            
            var mapping = JsonConvert.DeserializeObject<Dictionary<int, string>>(pageContent);
            var item = mapping.Where(x => categorySize >= x.Key).Max(x => x.Key);

            return this.responder.Respond(
                "commands.command.afccount",
                this.CommandSource,
                new object[]
                {
                    categorySize, mapping[item]
                });
        }
    }
}