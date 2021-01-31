namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System;
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
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
    [CommandInvocation("categorysize")]
    public class CategorySizeCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IMediaWikiApiHelper apiHelper;

        public CategorySizeCommand(
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

        [RequiredArguments(1)]
        [Help("<category>", "Returns the number of items in the provided category")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var categoryName = string.Join(" ", this.Arguments).Trim();
            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);

            try
            {
                var categorySize = mediaWikiApi.GetCategorySize(categoryName);

                return new[]
                {
                    new CommandResponse
                    {
                        Message = string.Format("[[Category:{0}]] has {1} items", categoryName, categorySize)
                    }
                };
            }
            catch (ArgumentException)
            {
                return new[]
                {
                    new CommandResponse
                    {
                        Message = string.Format("[[Category:{0}]] does not exist", categoryName)
                    }
                };
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }
        }
    }
}