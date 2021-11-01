namespace Helpmebot.Commands.Configuration
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class AfcCategoryConfiguration
    {
        public AfcCategoryConfiguration(
            IDictionary<string, string> rejectedCategories,
            IDictionary<string, string> declinedCategories,
            IDictionary<string, string> inReviewCategories,
            IDictionary<string, string> draftCategories,
            IDictionary<string, string> pendingCategories,
            IDictionary<string, string> inArticleSpaceCategories,
            IDictionary<string,string> speedyDeletionCategories
            )
        {
            this.RejectedCategories = new ReadOnlyDictionary<string, string>(rejectedCategories);
            this.DeclinedCategories = new ReadOnlyDictionary<string, string>(declinedCategories);
            this.InReviewCategories = new ReadOnlyDictionary<string, string>(inReviewCategories);
            this.DraftCategories = new ReadOnlyDictionary<string, string>(draftCategories);
            this.PendingCategories = new ReadOnlyDictionary<string, string>(pendingCategories);
            this.InArticleSpaceCategories = new ReadOnlyDictionary<string, string>(inArticleSpaceCategories);
            this.SpeedyDeletionCategories = new ReadOnlyDictionary<string, string>(speedyDeletionCategories);
        }

        public IDictionary<string, string> RejectedCategories { get; private set; }
        public IDictionary<string, string> DeclinedCategories { get; private set; }
        public IDictionary<string, string> InReviewCategories { get; private set; }
        public IDictionary<string, string> DraftCategories { get; private set; }
        public IDictionary<string, string> PendingCategories { get; private set; }
        public IDictionary<string, string> InArticleSpaceCategories { get; private set; }
        public IDictionary<string, string> SpeedyDeletionCategories { get; private set; }
    }
}