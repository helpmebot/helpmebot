namespace Helpmebot.CoreServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
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
        private readonly ILogger logger;

        private readonly Dictionary<string, CacheEntry> cache;

        public MediaWikiApiHelper(BotConfiguration config, IMediaWikiApiTypedFactory factory, ILogger logger)
        {
            this.config = config;
            this.factory = factory;
            this.logger = logger;
            this.cache = new Dictionary<string, CacheEntry>();
        }

        private class CacheEntry
        {
            public CacheEntry(IMediaWikiApi api)
            {
                this.API = api;
                this.LastCheck = DateTime.UtcNow;
                this.CheckoutCount = 0;
                this.ID = Guid.NewGuid();
            }

            public IMediaWikiApi API { get; set; }
            public DateTime LastCheck { get; set; }
            public int CheckoutCount { get; set; }
            public Guid ID { get; }
        } 
        
        public IMediaWikiApi GetApi(MediaWikiSite site)
        {
            lock (this.cache)
            {
                if (this.cache.ContainsKey(site.Api))
                {
                    var cachedValue = this.cache[site.Api];

                    if ((DateTime.UtcNow - cachedValue.LastCheck).Minutes > 30)
                    {
                        this.logger.DebugFormat("Refreshing login for MWAPI ID {0}", cachedValue.ID);
                        cachedValue.API.Login();
                        cachedValue.LastCheck = DateTime.UtcNow;
                    }

                    cachedValue.CheckoutCount++;

                    this.logger.DebugFormat(
                        "Checking out MWAPI ID {1}, new checkoutcount {0}",
                        cachedValue.CheckoutCount,
                        cachedValue.ID);

                    return cachedValue.API;
                }
                else
                {
                    var mwConfig = new MediaWikiConfiguration(
                        site.Api,
                        this.config.UserAgent,
                        site.Username,
                        site.Password);
                    var mediaWikiApi = this.factory.Create<IMediaWikiApi>(mwConfig);

                    var id = Guid.NewGuid();
                    var cacheEntry = new CacheEntry(mediaWikiApi);
                    cacheEntry.CheckoutCount++;
                    this.cache.Add(site.Api, cacheEntry);

                    this.logger.DebugFormat("Created new MWAPI for {0} with ID {1}", site.Api, id);

                    return mediaWikiApi;
                }
            }
        }

        public void Release(IMediaWikiApi api)
        {
            lock (this.cache)
            {
                var keyValuePair = this.cache.First(x => x.Value.API.Equals(api));
                var valueTuple = keyValuePair.Value;
                valueTuple.CheckoutCount--;
                this.logger.DebugFormat(
                    "Returned MWAPI ID {1}, new checkoutcount {0}",
                    valueTuple.CheckoutCount,
                    valueTuple.ID);
            }

            //this.factory.Release(api);
        }
    }
}