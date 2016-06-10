// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UrlShorteningServiceBase.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Services.UrlShortening
{
    using Castle.Core.Logging;

    using Helpmebot.Configuration;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The url shortening service base.
    /// </summary>
    public abstract class UrlShorteningServiceBase : IUrlShorteningService
    {
        /// <summary>
        /// The configuration helper.
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        ///     The short url cache repository.
        /// </summary>
        private readonly IShortUrlCacheRepository shortUrlCacheRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlShorteningServiceBase"/> class.
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
        protected UrlShorteningServiceBase(
            ILogger logger, 
            IShortUrlCacheRepository shortUrlCacheRepository, 
            IConfigurationHelper configurationHelper)
        {
            this.logger = logger;
            this.shortUrlCacheRepository = shortUrlCacheRepository;
            this.configurationHelper = configurationHelper;
        }

        /// <summary>
        /// Gets the configuration helper.
        /// </summary>
        protected IConfigurationHelper ConfigurationHelper
        {
            get
            {
                return this.configurationHelper;
            }
        }

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

            return this.shortUrlCacheRepository.GetShortUrl(longUrl, this.GetShortUrl);
        }

        /// <summary>
        /// The get short url.
        /// </summary>
        /// <param name="longUrl">
        /// The long url.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected abstract string GetShortUrl(string longUrl);
    }
}