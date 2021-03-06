namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
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

    [CommandFlag(Flags.Info)]
    [CommandInvocation("blockinfo")]
    public class BlockInformationCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IMediaWikiApiHelper apiHelper;

        public BlockInformationCommand(
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

        [Help("<target>", "Returns information about active blocks on the provided target")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiSiteObject = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSiteObject);

            var blockInfoResult = mediaWikiApi.GetBlockInformation(string.Join(" ", this.Arguments));

            return blockInfoResult.Select(x => new CommandResponse {Message = x.ToString()});
        }
    }
}