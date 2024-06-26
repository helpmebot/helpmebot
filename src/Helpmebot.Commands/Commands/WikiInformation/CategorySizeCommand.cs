namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.Bot.MediaWikiLib.Exceptions;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Info)]
    [CommandInvocation("categorysize")]
    [CommandInvocation("catsize")]
    [HelpSummary("Returns the number of items in the provided category")]
    public class CategorySizeCommand : CommandBase
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public CategorySizeCommand(
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

        [RequiredArguments(1)]
        [Help("<category>")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var categoryName = string.Join(" ", this.Arguments).Trim();

            var mediaWikiApi = this.apiHelper.GetApi(this.channelManagementService.GetBaseWiki(this.CommandSource));
            try
            {
                var categorySize = mediaWikiApi.GetCategorySize(categoryName);

                return this.responder.Respond(
                    "commands.command.catsize",
                    this.CommandSource,
                    new object[] { categoryName, categorySize });
            }
            catch (MissingObjectException)
            {
                return this.responder.Respond("commands.command.catsize.missing", this.CommandSource, categoryName);
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }
        }
    }
}