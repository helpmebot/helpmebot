namespace Helpmebot.Configuration
{
    using System.Collections.Generic;

    public class MediaWikiSiteConfiguration
    {
        public string Default { get; set; }
        public Dictionary<string, MediaWikiSite> Sites { get; set; }

        public class MediaWikiSite
        {
            public string Api { get; set; }
            public MediaWikiCredentials Credentials { get; set; }
        }

        public class MediaWikiCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}