// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UrlShorteningService.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services
{
    using System;
    using System.IO;
    using System.Net;
    using System.Web;

    using Castle.Core.Logging;

    using Helpmebot.Configuration;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///     Shortens URLs
    /// </summary>
    public class UrlShorteningService : IUrlShorteningService
    {
        #region Fields

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        ///     The short url cache repository.
        /// </summary>
        private readonly IShortUrlCacheRepository shortUrlCacheRepository;

        /// <summary>
        /// The configuration helper.
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="UrlShorteningService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="shortUrlCacheRepository">
        /// The short url cache repository.
        /// </param>
        /// <param name="configurationHelper">
        /// The configuration Helper.
        /// </param>
        public UrlShorteningService(ILogger logger, IShortUrlCacheRepository shortUrlCacheRepository, IConfigurationHelper configurationHelper)
        {
            this.logger = logger;
            this.shortUrlCacheRepository = shortUrlCacheRepository;
            this.configurationHelper = configurationHelper;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The shorten.
        /// </summary>
        /// <param name="longUrl">
        /// The long url.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string Shorten(string longUrl)
        {
            this.logger.InfoFormat("Getting short url for {0}...", longUrl);

            string result = longUrl;

            try
            {
                this.logger.DebugFormat("Searching cache for {0}", longUrl);
                ShortUrlCacheEntry cacheEntry = this.shortUrlCacheRepository.GetByLongUrl(longUrl);

                if (cacheEntry == null)
                {
                    this.logger.DebugFormat("Cache MISS for {0}", longUrl);

                    string shortUrl = this.GetShortUrl(longUrl);
                    cacheEntry = new ShortUrlCacheEntry { LongUrl = longUrl, ShortUrl = shortUrl };
                    this.shortUrlCacheRepository.Save(cacheEntry);
                    result = shortUrl;
                }
                else
                {
                    this.logger.DebugFormat("Cache HIT for {0}", longUrl);
                    result = cacheEntry.ShortUrl;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.Message, ex);
            }

            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get short url.
        /// </summary>
        /// <param name="longUrl">
        /// The long url.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetShortUrl(string longUrl)
        {
            var wrq =
                (HttpWebRequest)
                WebRequest.Create("http://is.gd/create.php?format=simple&url=" + HttpUtility.UrlEncode(longUrl));
            wrq.UserAgent = this.configurationHelper.CoreConfiguration.UserAgent;
            var wrs = (HttpWebResponse)wrq.GetResponse();
            if (wrs.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = wrs.GetResponseStream();

                if (responseStream == null)
                {
                    throw new WebException("Response stream is null.");
                }

                var sr = new StreamReader(responseStream);
                string shorturl = sr.ReadLine();
                return shorturl;
            }

            throw new WebException(wrs.StatusDescription);
        }

        #endregion
    }
}