namespace Helpmebot.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
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

        public AfcCountCommand(
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

        [Help("", "Returns the number of AfC submissions awaiting review")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var categoryName = "Pending AfC submissions";
            
            // force this into the default
            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(string.Empty);
            
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            var categorySize = mediaWikiApi.GetCategorySize(categoryName);

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
                        "[[Category:{0}]] has {1} items - {2}",
                        categoryName,
                        categorySize,
                        mapping[item])
                }
            };
        }
    }
}