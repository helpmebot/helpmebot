namespace Helpmebot.CoreServices.Services.UrlShortening
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.TypedFactories;
    using Stwalkerster.Bot.MediaWikiLib.Configuration;
    using Stwalkerster.Bot.MediaWikiLib.Exceptions;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public class WikimediaUrlShorteningService : UrlShorteningServiceBase
    {
        private readonly ILogger logger;
        private readonly IMediaWikiApiTypedFactory apiTypedFactory;
        private readonly UrlShorteningServiceBase secondaryShortener;
        private readonly List<Regex> allowedDomains;
        private readonly MediaWikiConfiguration mediaWikiConfig;

        public WikimediaUrlShorteningService(
            ILogger logger,
            IShortUrlCacheService shortUrlCacheService,
            IMediaWikiApiTypedFactory apiTypedFactory,
            BotConfiguration configuration,
            IUrlShorteningService secondaryShortener,
            WikimediaUrlShortnerConfiguration shortenerConfiguration)
            : base(logger, shortUrlCacheService)
        {
            this.logger = logger;
            this.apiTypedFactory = apiTypedFactory;
            this.secondaryShortener = (UrlShorteningServiceBase) secondaryShortener;
            this.allowedDomains = shortenerConfiguration.AllowedDomains.Select(x =>
            {
                try
                {
                    return new Regex(x);
                }
                catch (Exception ex)
                {
                    this.logger.ErrorFormat(ex, "Could not parse regex {0} for URL shortener, skipping", x);
                    return null;
                }
            }).Where(x => x != null).ToList();
            
            this.mediaWikiConfig = new MediaWikiConfiguration(
                shortenerConfiguration.MediaWikiApiEndpoint,
                configuration.UserAgent,
                shortenerConfiguration.MediaWikiApiUsername,
                shortenerConfiguration.MediaWikiApiPassword);
        }

        protected internal override string GetShortUrl(string longUrl)
        {
            // check for allowed domains
            var host = new Uri(longUrl).Host;
            var match = false;
            foreach (var regex in this.allowedDomains)
            {
                if (regex.IsMatch(host))
                {
                    match = true;
                    break;
                }
            }

            if (!match)
            {
                this.logger.DebugFormat("Url shortening request for {0} did not match allowed domains; deferring to secondary");
                return this.secondaryShortener.GetShortUrl(longUrl);
            }
            
            IMediaWikiApi mediaWikiApi = null;
            try
            {
                mediaWikiApi = this.apiTypedFactory.Create<IMediaWikiApi>(this.mediaWikiConfig);
                mediaWikiApi.Login();

                return mediaWikiApi.ShortenUrl(longUrl);
            }
            catch (GeneralMediaWikiApiException ex)
            {
                this.logger.Debug(ex.ApiResponse);
                this.logger.ErrorFormat(ex, "Error shortening url {0} with WMF shortener", longUrl);
                return this.secondaryShortener.GetShortUrl(longUrl);
            }
            finally
            {
                if (mediaWikiApi != null)
                {
                    this.apiTypedFactory.Release(mediaWikiApi);
                }
            }
        }
    }
}