namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("page")]
    [CommandFlag(Flags.Info)]
    public class PageInfoCommand : CommandBase
    {
        private readonly ILinkerService linkerService;
        private readonly ISession databaseSession;
        private readonly IMediaWikiApiHelper apiHelper;

        public PageInfoCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ILinkerService linkerService,
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
            this.linkerService = linkerService;
            this.databaseSession = databaseSession;
            this.apiHelper = apiHelper;
        }

        [Help("<page title>", "Provides basic information on the provided page")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var pageTitle = this.GetPageTitle();
            
            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
            
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            try
            {
                var pageData = mediaWikiApi.GetPageInformation(pageTitle);
                var responses = new List<CommandResponse>();

                if (pageData.RedirectedFrom.Any())
                {
                    responses.Add(
                        new CommandResponse
                        {
                            Message = string.Format("Redirected from: {0}", string.Join(", ", pageData.RedirectedFrom))
                        });
                }

                if (pageData.Missing)
                {
                    responses.Add(
                        new CommandResponse
                        {
                            Message = string.Format("The page {0} does not exist.", pageData.Title)
                        });
                }
                else
                {
                    if (pageData.Title == null)
                    {
                        responses.Add(
                            new CommandResponse
                            {
                                Message = "No page data was found, circular redirect?"
                            });
                    }
                    else
                    {
                        responses.Add(
                            new CommandResponse
                            {
                                Message = string.Format(
                                    "Page [[{0}]] was last edited by [[User:{1}]] at {2:u}. The edit summary was: {3}",
                                    pageData.Title,
                                    pageData.LastRevUser,
                                    pageData.Touched,
                                    pageData.LastRevComment)
                            });
                    }
                }

                foreach (var protection in pageData.Protection)
                {
                    responses.Add(
                        new CommandResponse
                        {
                            Message = string.Format(
                                "[[{0}]] is protected against {1} actions at the {3} level {2}.",
                                pageData.Title,
                                protection.Type,
                                protection.Expiry.HasValue
                                    ? string.Format("until {0:u}", protection.Expiry.Value)
                                    : "forever",
                                protection.Level)
                        });
                }

                return responses;
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }
        }

        private string GetPageTitle()
        {
            var naiveTitle = string.Join(" ", this.Arguments);
            var parsedTitles = this.linkerService.ParseMessageForLinks(naiveTitle);

            return parsedTitles.Count == 0 ? naiveTitle : parsedTitles.First();
        }
    }
}