namespace Helpmebot.Model
{
    using System;
    using System.Collections.Generic;

    public class DraftStatus
    {
        public DraftStatus(string page)
        {
            this.Page = page;
            this.RejectionCategories = new List<string>();
            this.DeclineCategories = new List<string>();
        }

        public DraftStatusCode StatusCode { get; set; }
        
        public string Page { get; set; }
        
        public bool DuplicatesExistingArticle { get; set; }
        
        public bool SubmissionInArticleSpace { get; set; }
        
        public bool Misplaced { get; set; }
        
        public IList<string> RejectionCategories { get; set; }
     
        public IList<string> DeclineCategories { get; set; }
        
        public IList<string> SpeedyDeletionCategories { get; set; }
        
        public DateTime? SubmissionDate { get; set; }
    }
}