namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System;
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
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
        private readonly IResponder responder;

        public CategorySizeCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
            IMediaWikiApiHelper apiHelper,
            IResponder responder) : base(
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
            this.responder = responder;
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

                return this.responder.Respond("commands.command.catsize", this.CommandSource, new object[] { categoryName, categorySize });
            }
            catch (ArgumentException)
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