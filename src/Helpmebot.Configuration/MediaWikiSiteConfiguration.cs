namespace Helpmebot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MediaWikiSiteConfiguration
    {
        public string Default { get; set; }
        public List<MediaWikiSite> Sites { get; set; }

        public MediaWikiSite GetSite(string id, bool fallback = false)
        {
            if (!fallback && id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            
            var requested = this.Sites.FirstOrDefault(x => x.WikiId == id);

            if (requested == null && fallback)
            {
                requested = this.Sites.FirstOrDefault(x => x.WikiId == this.Default);
            }
            
            return requested;
        }

        public class MediaWikiSite
        {
            public string WikiId { get; set; }
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