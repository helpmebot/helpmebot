namespace Helpmebot.Commands.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Commands.Model;
    using Helpmebot.Commands.Services.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.Model;
    using Stwalkerster.Bot.MediaWikiLib.Model;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using AfcCategoryConfiguration = Helpmebot.Commands.Configuration.AfcCategoryConfiguration;

    public class DraftStatusService : IDraftStatusService
    {
        private readonly AfcCategoryConfiguration categoryConfiguration;
        private readonly ILogger logger;

        public DraftStatusService(AfcCategoryConfiguration categoryConfiguration, ILogger logger)
        {
            this.categoryConfiguration = categoryConfiguration;
            this.logger = logger;
        }

        public DraftStatus GetDraftStatus(IMediaWikiApi mediaWikiApi, string page)
        {
            var status = new DraftStatus(page);
            
            // retrieve the categories of the page
            var categorySet = mediaWikiApi.GetCategoriesOfPage(page);

            var speedyCategories = this.GetSpeedyCategories(categorySet);
            if (speedyCategories.Any())
            {
                status.StatusCode = status.StatusCode == DraftStatusCode.Unknown
                    ? DraftStatusCode.SpeedyDeletion
                    : status.StatusCode;

                status.SpeedyDeletionCategories = speedyCategories;
            }

            if (this.IsBeingReviewedNow(categorySet))
            {
                status.StatusCode = status.StatusCode == DraftStatusCode.Unknown
                    ? DraftStatusCode.InReviewNow
                    : status.StatusCode;
            }

            if (this.IsInArticleSpace(categorySet))
            {
                status.StatusCode = status.StatusCode == DraftStatusCode.Unknown
                    ? DraftStatusCode.Pending
                    : status.StatusCode;

                status.SubmissionInArticleSpace = true;
            }
            
            if (this.IsPendingReview(categorySet))
            {
                status.StatusCode = status.StatusCode == DraftStatusCode.Unknown
                    ? DraftStatusCode.Pending
                    : status.StatusCode;

                if (this.DuplicatesExisting(categorySet))
                {
                    status.DuplicatesExistingArticle = true;
                }
            }

            if (this.IsDraft(categorySet))
            {
                status.StatusCode = status.StatusCode == DraftStatusCode.Unknown
                    ? DraftStatusCode.Draft
                    : status.StatusCode;
            }

            var rejectCategories = this.GetRejectionCategories(categorySet);
            if (rejectCategories.Any())
            {
                status.StatusCode = status.StatusCode == DraftStatusCode.Unknown
                    ? DraftStatusCode.Rejected
                    : status.StatusCode;

                status.RejectionCategories = rejectCategories;
            }

            var declineCategories = this.GetDeclineCategories(categorySet);
            if (declineCategories.Any())
            {
                status.StatusCode = status.StatusCode == DraftStatusCode.Unknown
                    ? DraftStatusCode.Declined
                    : status.StatusCode;

                status.DeclineCategories = declineCategories;
            }

            if (status.StatusCode == DraftStatusCode.Unknown)
            {
                this.logger.WarnFormat("Draft [[{0}]] reported status unknown - categories: {0}", string.Join("|", categorySet));
            }

            status.SubmissionDate = this.GetSubmissionDate(categorySet);

            return status;
        }

        public int GetPendingDraftCount(IMediaWikiApi mediaWikiApi)
        {
            var categoryName = this.categoryConfiguration.PendingCategories.First().Key;

            if (categoryName.StartsWith("Category:"))
            {
                categoryName = categoryName.Substring("Category:".Length);
            }
            
            var categorySize = mediaWikiApi.GetCategorySize(categoryName);

            return categorySize;
        }

        public DateTime? GetOldestDraft(IMediaWikiApi mediaWikiApi)
        {
            var pagesInCategory = mediaWikiApi.GetPagesInCategory(
                this.categoryConfiguration.PendingCategories.First().Key,
                "max",
                true);
            
            var pendingDated = pagesInCategory.Values
                .Where(x => x.StartsWith("P") && x.Length >= 9)
                .Select(x => x.Substring(1, 8))
                .ToList();

            var expectedDuration = pendingDated.Skip((int)Math.Floor(pendingDated.Count * 0.05)).FirstOrDefault();

            if (expectedDuration == null)
            {
                return null;
            }

            return DateTime.ParseExact(expectedDuration, "yyyyMMdd", CultureInfo.InvariantCulture);
        }
        
        private DateTime? GetSubmissionDate(IDictionary<string, PageCategoryProperties> categorySet)
        {
            var datedCategoryPrefix = "Category:AfC submissions by date/";
            
            var dateStrings = categorySet
                .Where(x => x.Key.StartsWith(datedCategoryPrefix))
                .Select(x => x.Key.Substring(datedCategoryPrefix.Length));

            var dates = dateStrings
                .Select(x => DateTime.ParseExact(x, "dd MMMM yyyy", CultureInfo.InvariantCulture))
                .ToList();

            return dates.Any() ? dates.Max() : (DateTime?)null;
        }

        private bool IsInArticleSpace(IDictionary<string, PageCategoryProperties> categorySet)
        {
            return categorySet.Keys
                .Intersect(this.categoryConfiguration.InArticleSpaceCategories.Keys.ToList())
                .Any();
        }

        private bool DuplicatesExisting(IDictionary<string, PageCategoryProperties> categorySet)
        {
            var status = false;
            var catSet = categorySet.Keys.Intersect(this.categoryConfiguration.PendingCategories.Keys.ToList()).ToList();
            if (catSet.Any())
            {
                status = true;
                var pageCategoryProperties = categorySet.First(x => x.Key == catSet.First()).Value;
                status &= pageCategoryProperties.SortKey.StartsWith("D");
            }

            return status;
        }

        private bool IsBeingReviewedNow(IDictionary<string, PageCategoryProperties> categorySet)
        {
            return categorySet.Keys
                .Intersect(this.categoryConfiguration.InReviewCategories.Keys.ToList())
                .Any();
        }

        private bool IsDraft(IDictionary<string, PageCategoryProperties> categorySet)
        {
            return categorySet.Keys
                .Intersect(this.categoryConfiguration.DraftCategories.Keys.ToList())
                .Any();
        }

        private bool IsPendingReview(IDictionary<string, PageCategoryProperties> categorySet)
        {
            var status = false;
            var catSet = categorySet.Keys.Intersect(this.categoryConfiguration.PendingCategories.Keys.ToList());
            if (catSet.Any())
            {
                status = true;
                var pageCategoryProperties = categorySet.First().Value;
                status &= !pageCategoryProperties.SortKey.StartsWith("C");
                status &= !pageCategoryProperties.SortKey.StartsWith("R");
                status &= !pageCategoryProperties.SortKey.StartsWith("M");
            }

            return status;
        }

        private IList<string> GetRejectionCategories(IDictionary<string, PageCategoryProperties> categorySet)
        {
            return categorySet.Keys
                .Intersect(this.categoryConfiguration.RejectedCategories.Keys.ToList())
                .ToList();
        }

        private IList<string> GetDeclineCategories(IDictionary<string, PageCategoryProperties> categorySet)
        {
            return categorySet.Keys
                .Intersect(this.categoryConfiguration.DeclinedCategories.Keys.ToList())
                .ToList();
        }
        
        private IList<string> GetSpeedyCategories(IDictionary<string, PageCategoryProperties> categorySet)
        {
            return categorySet.Keys
                .Intersect(this.categoryConfiguration.SpeedyDeletionCategories.Keys.ToList())
                .ToList();
        }
    }
}