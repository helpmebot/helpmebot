namespace Helpmebot.Commands.Configuration
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class AfcCategoryConfiguration
    {
        public IDictionary<string, string> RejectedCategories { get; set; }
        public IDictionary<string, string> DeclinedCategories { get; set; }
        public IDictionary<string, string> InReviewCategories { get; set; }
        public IDictionary<string, string> DraftCategories { get; set; }
        public IDictionary<string, string> PendingCategories { get; set; }
        public IDictionary<string, string> InArticleSpaceCategories { get; set; }
        public IDictionary<string, string> SpeedyDeletionCategories { get; set; }
    }
}