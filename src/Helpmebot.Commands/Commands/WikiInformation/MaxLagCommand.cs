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

    [CommandInvocation("maxlag")]
    [CommandFlag(Flags.Info)]
    [HelpSummary("Returns the maximum replication lag on the channel's MediaWiki instance")]
    public class MaxLagCommand : CommandBase
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public MaxLagCommand(
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
            var mediaWikiSiteObject = this.channelManagementService.GetBaseWiki(this.CommandSource);
            
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSiteObject);
            try
            {
                var maxLag = mediaWikiApi.GetMaxLag();
                return this.responder.Respond("commands.command.maxlag", this.CommandSource, maxLag);
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }
        }
    }
}