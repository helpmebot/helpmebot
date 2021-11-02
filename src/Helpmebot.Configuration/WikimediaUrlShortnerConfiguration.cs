namespace Helpmebot.Configuration
{
    using System.Collections.Generic;

    public class WikimediaUrlShortnerConfiguration
    {
        public string MediaWikiApiEndpoint { get; set; }
        public string MediaWikiApiUsername { get; set; }
        public string MediaWikiApiPassword { get; set; }
        public List<string> AllowedDomains { get; set; }
    }
}