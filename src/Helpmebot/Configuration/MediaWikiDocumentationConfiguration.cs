namespace Helpmebot.Configuration
{
    public class MediaWikiDocumentationConfiguration
    {
        private readonly string apiBase;
        private readonly string username;
        private readonly string password;
        private readonly string documentationPrefix;

        public MediaWikiDocumentationConfiguration(
            string apiBase,
            string username,
            string password,
            string documentationPrefix)
        {
            this.apiBase = apiBase;
            this.username = username;
            this.password = password;
            this.documentationPrefix = documentationPrefix;
        }

        public string ApiBase
        {
            get { return this.apiBase; }
        }

        public string Username
        {
            get { return this.username; }
        }

        internal string Password
        {
            get { return this.password; }
        }

        public string DocumentationPrefix
        {
            get { return this.documentationPrefix; }
        }
    }
}