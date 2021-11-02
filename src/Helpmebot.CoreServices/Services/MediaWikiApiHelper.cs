namespace Helpmebot.CoreServices.Services
{
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.TypedFactories;
    using Stwalkerster.Bot.MediaWikiLib.Configuration;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public class MediaWikiApiHelper : IMediaWikiApiHelper
    {
        private readonly BotConfiguration config;
        private readonly IMediaWikiApiTypedFactory factory;

        public MediaWikiApiHelper(BotConfiguration config, IMediaWikiApiTypedFactory factory)
        {
            this.config = config;
            this.factory = factory;
        }

        public IMediaWikiApi GetApi(MediaWikiSite site)
        {
            var mwConfig = new MediaWikiConfiguration(site.Api, this.config.UserAgent, site.Username, site.Password);

            return this.factory.Create<IMediaWikiApi>(mwConfig);
        }        
        
        public IMediaWikiApi GetApi(string apiUrl, string username, string password)
        {
            var mwConfig = new MediaWikiConfiguration(apiUrl, this.config.UserAgent, username, password);

            return this.factory.Create<IMediaWikiApi>(mwConfig);
        }

        public void Release(IMediaWikiApi api)
        {
            this.factory.Release(api);
        }
    }
}