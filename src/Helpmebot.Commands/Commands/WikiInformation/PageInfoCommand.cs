namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
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
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public PageInfoCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ILinkerService linkerService,
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
            this.linkerService = linkerService;
            this.apiHelper = apiHelper;
            this.responder = responder;
            this.channelManagementService = channelManagementService;
        }

        [Help("<page title>", "Provides basic information on the provided page")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var pageTitle = this.GetPageTitle();
            
            var mediaWikiSite = this.channelManagementService.GetBaseWiki(this.CommandSource);
            
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            try
            {
                var pageData = mediaWikiApi.GetPageInformation(pageTitle);
                var responses = new List<CommandResponse>();

                if (pageData.RedirectedFrom.Any())
                {
                    responses.AddRange(
                        this.responder.Respond(
                            "commands.command.pageinfo.redirect",
                            this.CommandSource,
                            string.Join(", ", pageData.RedirectedFrom)));
                }

                if (pageData.Missing)
                {
                    responses.AddRange(
                        this.responder.Respond(
                            "commands.command.pageinfo.missing",
                            this.CommandSource,
                            pageData.Title));
                }
                else
                {
                    if (pageData.Title == null)
                    {
                        responses.AddRange(
                            this.responder.Respond(
                                "commands.command.pageinfo.circular",
                                this.CommandSource));
                    }
                    else
                    {

                        responses.AddRange(
                            this.responder.Respond(
                                "commands.command.pageinfo.lastedit",
                                this.CommandSource,
                                new object[]
                                {
                                    pageData.Title,
                                    pageData.LastRevUser,
                                    pageData.Touched,
                                    pageData.LastRevComment
                                }));
                    }
                }

                foreach (var protection in pageData.Protection)
                {
                    string messageKey;
                    if (protection.Expiry.HasValue)
                    {
                        messageKey = "commands.command.pageinfo.protection.indef";
                    }
                    else
                    {
                        messageKey = "commands.command.pageinfo.protection";
                    }

                    responses.AddRange(
                        this.responder.Respond(
                            messageKey,
                            this.CommandSource,
                            new object[]
                            {
                                pageData.Title,
                                protection.Type,
                                protection.Expiry.GetValueOrDefault(),
                                protection.Level
                            }));
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