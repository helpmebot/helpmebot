namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using CoreServices.Services.Messages.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Model;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Info)]
    [CommandInvocation("blockinfo")]
    [HelpSummary("Returns information about active blocks on the provided target")]
    public class BlockInformationCommand : CommandBase
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IChannelManagementService channelManagementService;
        private readonly IResponder responder;

        public BlockInformationCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMediaWikiApiHelper apiHelper,
            IChannelManagementService channelManagementService,
            IResponder responder) : base(
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
            this.responder = responder;
        }

        [Help("<target>")]
        [RequiredArguments(1)]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var username = string.Join(" ", this.Arguments);
            List<BlockInformation> blockInfoResult;
            
            var mediaWikiApi = this.apiHelper.GetApi(this.channelManagementService.GetBaseWiki(this.CommandSource));
            try
            {
                blockInfoResult = mediaWikiApi.GetBlockInformation(username).ToList();
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }

            if (blockInfoResult.Any())
            {
                return blockInfoResult.Select(x => new CommandResponse { Message = x.ToString() });
            }

            return this.responder.Respond("commands.command.blockinfo.none", this.CommandSource, username);
        }
    }
}