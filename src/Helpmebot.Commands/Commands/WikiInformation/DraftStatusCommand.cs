namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Commands.Model;
    using Helpmebot.Commands.Services.Interfaces;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;
    using AfcCategoryConfiguration = Helpmebot.Commands.Configuration.AfcCategoryConfiguration;

    [CommandInvocation("reviewstatus")]
    [CommandInvocation("draftstatus")]
    [CommandInvocation("draft")]
    [CommandFlag(Flags.Info)]
    public class DraftStatusCommand : CommandBase
    {
        private readonly AfcCategoryConfiguration categoryConfiguration;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly ISession databaseSession;
        private readonly IDraftStatusService draftStatusService;
        private readonly ILinkerService linkerService;
        private readonly IUrlShorteningService urlShorteningService;

        public DraftStatusCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            AfcCategoryConfiguration categoryConfiguration,
            IMediaWikiApiHelper apiHelper,
            ISession databaseSession,
            IDraftStatusService draftStatusService, ILinkerService linkerService, IUrlShorteningService urlShorteningService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.categoryConfiguration = categoryConfiguration;
            this.apiHelper = apiHelper;
            this.databaseSession = databaseSession;
            this.draftStatusService = draftStatusService;
            this.linkerService = linkerService;
            this.urlShorteningService = urlShorteningService;
        }

        [Help("<draft>", "Provides the current status of a draft")]
        [RequiredArguments(1)]
        protected override IEnumerable<CommandResponse> Execute()
        {
            // check current channel is enwiki-capable
            var mediaWikiSite = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);
            if (!mediaWikiSite.Api.Contains("en.wikipedia"))
            {
                throw new CommandErrorException(
                    "This command is only supported on channels configured for the English Wikipedia.");
            }
            
            var stopwatch = Stopwatch.StartNew();
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
            try
            {
                // T1961 - add a fuzzy search to this
                var page = this.OriginalArguments;

                var draftStatus = this.draftStatusService.GetDraftStatus(mediaWikiApi, page);

                var reportedDecline = false;
                var reportedRejected = false;

                var draftLink = string.Format(
                    "( {0} ) ",
                    this.urlShorteningService.Shorten(
                        this.linkerService.ConvertWikilinkToUrl(this.CommandSource, draftStatus.Page)));

                switch (draftStatus.StatusCode)
                {
                    case DraftStatusCode.Unknown:
                        // Umm?
                        yield return new CommandResponse
                        {
                            Message = string.Format("[[{0}]] {1}is not currently submitted for review.", draftStatus.Page, draftLink)
                        };
                        yield break;

                    case DraftStatusCode.Draft:
                        yield return new CommandResponse
                        {
                            Message = string.Format("[[{0}]] {1}is not currently submitted for review.", draftStatus.Page, draftLink)
                        };
                        break;
                    case DraftStatusCode.Pending:
                        yield return new CommandResponse
                        {
                            Message = string.Format("[[{0}]] {1}has been submitted for review.", draftStatus.Page, draftLink)
                        };
                        break;
                    case DraftStatusCode.InReviewNow:
                        yield return new CommandResponse
                        {
                            Message = string.Format("[[{0}]] {1}is currently being reviewed.", draftStatus.Page, draftLink)
                        };
                        break;

                    case DraftStatusCode.SpeedyDeletion:
                        var speedyReason = this.GetMessageFromCategorySet(
                            this.categoryConfiguration.SpeedyDeletionCategories,
                            draftStatus.SpeedyDeletionCategories,
                            "nominated for speedy deletion");

                        yield return new CommandResponse
                        {
                            Message = string.Format( "[[{0}]] {2}has been {1}", draftStatus.Page, speedyReason, draftLink)
                        };
                        break;

                    case DraftStatusCode.Rejected:
                        var rejectReason = this.GetMessageFromCategorySet(
                            this.categoryConfiguration.RejectedCategories,
                            draftStatus.RejectionCategories,
                            "rejected");

                        yield return new CommandResponse
                        {
                            Message = string.Format(
                                "[[{0}]] {2}has been {1}",
                                draftStatus.Page,
                                rejectReason,
                                draftLink)
                        };
                        reportedRejected = true;
                        break;

                    case DraftStatusCode.Declined:
                        var declineReason = this.GetMessageFromCategorySet(
                            this.categoryConfiguration.DeclinedCategories,
                            draftStatus.DeclineCategories,
                            "declined");

                        yield return new CommandResponse
                        {
                            Message = string.Format("[[{0}]] {2}has been {1}", draftStatus.Page, declineReason, draftLink)
                        };
                        reportedDecline = true;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }


                if (!reportedRejected && draftStatus.RejectionCategories.Any())
                {
                    var declineReason = this.GetMessageFromCategorySet(
                        this.categoryConfiguration.RejectedCategories,
                        draftStatus.RejectionCategories,
                        "rejected");

                    yield return new CommandResponse
                        { Message = string.Format("This draft has also been {0}", declineReason) };
                }

                if (!reportedDecline && draftStatus.DeclineCategories.Any())
                {
                    var declineReason = this.GetMessageFromCategorySet(
                        this.categoryConfiguration.DeclinedCategories,
                        draftStatus.DeclineCategories,
                        "declined");

                    yield return new CommandResponse
                        { Message = string.Format("This draft has also been {0}", declineReason) };
                }

                if (draftStatus.SubmissionDate.HasValue && draftStatus.StatusCode == DraftStatusCode.Pending)
                {
                    var (oldestDraft, categorySize) = this.draftStatusService.GetOldestDraft(mediaWikiApi);

                    var extraDataComment = string.Empty;

                    if (oldestDraft.HasValue)
                    {
                        var totalDays = (DateTime.UtcNow - oldestDraft.Value).TotalDays;
                        var oldestDuration = this.DescribeDuration(totalDays);
                        extraDataComment = string.Format(
                            " Please be patient - review may take more than {0}. There are currently {1} drafts awaiting review by volunteers; drafts are not reviewed in any particular order.",
                            oldestDuration,
                            categorySize);
                    }

                    var response = string.Format(
                        "This submission was submitted on {0:yyyy-MM-dd} ({1} ago).{2}",
                        draftStatus.SubmissionDate,
                        this.DescribeDuration((DateTime.UtcNow - draftStatus.SubmissionDate.Value).TotalDays),
                        extraDataComment);

                    yield return new CommandResponse { Message = response };
                }


                if (draftStatus.SubmissionInArticleSpace)
                {
                    yield return new CommandResponse { Message = "This submission is in the article namespace." };
                }

                if (draftStatus.DuplicatesExistingArticle)
                {
                    yield return new CommandResponse
                        { Message = "This draft has the same name as an existing article." };
                }

            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
                
                stopwatch.Stop();
                this.Logger.Debug($"draftstatus command exec finished in {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        private string DescribeDuration(double totalDays)
        {
            var days = (int) Math.Floor(totalDays);
            var weeks = (int) Math.Round(totalDays / 7, 0);

            // arbitrary point to switch from days to weeks 
            if (days <= 11)
            {
                return string.Format("{0} day{1}", days, days != 1 ? "s" : string.Empty);
            }
            else
            {
                return string.Format("{0} week{1}", weeks, weeks != 1 ? "s" : string.Empty);
            }
        }

        private string GetMessageFromCategorySet(IDictionary<string,string> lookup, ICollection<string> thisPageSet, string verb)
        {
            var set = lookup.Where(x => thisPageSet.Contains(x.Key) && !string.IsNullOrEmpty(x.Value))
                .ToDictionary(x => x.Key, y => y.Value);   
            
            if (!set.Any())
            {
                return string.Format("{0}.", verb);
            }

            if (set.Count == 1)
            {
                return string.Format("{0} {1}.", verb, set.First().Value);
            }
            
            var data = set.Select(x => x.Value).ToList();
            var firstGroup = data.Take(data.Count - 1);
            var last = data.Last();

            return string.Format("{0} {1}, and {2}.", verb, string.Join(", ", firstGroup), last);
        }
    }
}
