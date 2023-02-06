namespace Helpmebot.CoreServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.TypedFactories;
    using Stwalkerster.Bot.MediaWikiLib.Configuration;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public class MediaWikiApiHelper : IMediaWikiApiHelper
    {
        public const string WikipediaEnglish = "enwiki";
        
        private readonly BotConfiguration botConfig;
        private readonly IMediaWikiApiTypedFactory factory;
        private readonly ILogger logger;
        private readonly MediaWikiSiteConfiguration mwConfig;

        private readonly Dictionary<string, CacheEntry> cache;

        public MediaWikiApiHelper(BotConfiguration botConfig, IMediaWikiApiTypedFactory factory, ILogger logger, MediaWikiSiteConfiguration mwConfig)
        {
            this.botConfig = botConfig;
            this.factory = factory;
            this.logger = logger;
            this.mwConfig = mwConfig;
            this.cache = new Dictionary<string, CacheEntry>();
        }

        private class CacheEntry
        {
            public CacheEntry(IMediaWikiApi api)
            {
                this.Api = api;
                this.LastCheck = DateTime.UtcNow;
                this.CheckoutCount = 0;
                this.Id = Guid.NewGuid();
            }

            public IMediaWikiApi Api { get; }
            public DateTime LastCheck { get; set; }
            public int CheckoutCount { get; set; }
            public Guid Id { get; }
        }

        public IMediaWikiApi GetApi(string siteId, bool fallback = true)
        {
            MediaWikiSiteConfiguration.MediaWikiSite site;
            
            if (string.IsNullOrWhiteSpace(siteId))
            {
                site = this.mwConfig.GetSite(null, true);
            }
            else
            {
                site = this.mwConfig.GetSite(siteId, fallback);
            }

            if (site == null)
            {
                throw new NullReferenceException("Unable to retrieve API configuration");
            }
            
            return this.GetApiInternal(site);
        }
        
        public void Release(IMediaWikiApi api)
        {
            lock (this.cache)
            {
                var keyValuePair = this.cache.First(x => x.Value.Api.Equals(api));
                var valueTuple = keyValuePair.Value;
                valueTuple.CheckoutCount--;
                this.logger.DebugFormat(
                    "Returned MWAPI ID {1}, new checkoutcount {0}",
                    valueTuple.CheckoutCount,
                    valueTuple.Id);
            }

            //this.factory.Release(api);
        }
        
        private IMediaWikiApi GetApiInternal(MediaWikiSiteConfiguration.MediaWikiSite site)
        {
            lock (this.cache)
            {
                if (this.cache.ContainsKey(site.Api))
                {
                    var cachedValue = this.cache[site.Api];

                    if ((DateTime.UtcNow - cachedValue.LastCheck).Minutes > 30)
                    {
                        this.logger.DebugFormat("Refreshing login for MWAPI ID {0}", cachedValue.Id);
                        cachedValue.Api.Login();
                        cachedValue.LastCheck = DateTime.UtcNow;
                    }

                    cachedValue.CheckoutCount++;

                    this.logger.DebugFormat(
                        "Checking out MWAPI ID {1}, new checkoutcount {0}",
                        cachedValue.CheckoutCount,
                        cachedValue.Id);

                    return cachedValue.Api;
                }
                else
                {
                    var conf = new MediaWikiConfiguration(
                        site.Api,
                        this.botConfig.UserAgent,
                        site.Credentials.Username,
                        site.Credentials.Password);
                    var mediaWikiApi = this.factory.Create<IMediaWikiApi>(conf);

                    var id = Guid.NewGuid();
                    var cacheEntry = new CacheEntry(mediaWikiApi);
                    cacheEntry.CheckoutCount++;
                    this.cache.Add(site.Api, cacheEntry);

                    this.logger.DebugFormat("Created new MWAPI for {0} with ID {1}", site.Api, id);

                    return mediaWikiApi;
                }
            }
        }
    }
}