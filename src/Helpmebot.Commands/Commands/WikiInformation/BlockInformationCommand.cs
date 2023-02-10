namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Info)]
    [CommandInvocation("blockinfo")]
    [HelpSummary("Returns information about active blocks on the provided target")]
    public class BlockInformationCommand : CommandBase
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IChannelManagementService channelManagementService;

        public BlockInformationCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMediaWikiApiHelper apiHelper,
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
            this.channelManagementService = channelManagementService;
        }

        [Help("<target>")]
        [RequiredArguments(1)]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiApi = this.apiHelper.GetApi(this.channelManagementService.GetBaseWiki(this.CommandSource));

            var blockInfoResult = mediaWikiApi.GetBlockInformation(string.Join(" ", this.Arguments));

            return blockInfoResult.Select(x => new CommandResponse {Message = x.ToString()});
        }
    }
}