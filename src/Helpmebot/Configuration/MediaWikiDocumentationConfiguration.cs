namespace Helpmebot.Configuration
{
    public class MediaWikiDocumentationConfiguration
    {
        private readonly int siteId;
        private readonly string documentationPrefix;

        public MediaWikiDocumentationConfiguration(
            int siteId,
            string documentationPrefix)
        {
            this.siteId = siteId;
            this.documentationPrefix = documentationPrefix;
        }

        public string DocumentationPrefix
        {
            get { return this.documentationPrefix; }
        }

        public int SiteId
        {
            get { return this.siteId; }
        }
    }
}