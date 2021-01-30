namespace Helpmebot.Configuration
{
    public class MediaWikiDocumentationConfiguration
    {
        private readonly int siteId;
        private readonly string documentationPrefix;
        private readonly string humanDocumentationPrefix;

        public MediaWikiDocumentationConfiguration(
            int siteId,
            string documentationPrefix,
            string humanDocumentationPrefix)
        {
            this.siteId = siteId;
            this.documentationPrefix = documentationPrefix;
            this.humanDocumentationPrefix = humanDocumentationPrefix;
        }

        public string DocumentationPrefix
        {
            get { return this.documentationPrefix; }
        }

        public string HumanDocumentationPrefix
        {
            get { return this.humanDocumentationPrefix; }
        }

        public int SiteId
        {
            get { return this.siteId; }
        }
    }
}