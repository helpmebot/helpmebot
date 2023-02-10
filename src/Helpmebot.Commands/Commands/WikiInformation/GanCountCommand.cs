namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Info)]
    [CommandInvocation("gancount")]
    [HelpSummary("Returns the number of GAN submissions awaiting review")]
    public class GanCountCommand : CommandBase
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public GanCountCommand(
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
        
        protected override IEnumerable<CommandResponse> Execute()
        {
            var categoryName = "Good article nominees awaiting review";
            var mediaWikiSite = this.channelManagementService.GetBaseWiki(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            var categorySize = mediaWikiApi.GetCategorySize(categoryName);
            this.apiHelper.Release(mediaWikiApi);

            return this.responder.Respond(
                "commands.command.gancount",
                this.CommandSource,
                new object[] { categoryName, categorySize });
        }
    }
}