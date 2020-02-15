// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsGdUrlShorteningService.cs" company="Helpmebot Development Team">
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
    using System.IO;
    using System.Net;
    using System.Web;

    using Castle.Core.Logging;

    using Helpmebot.Configuration;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///     Shortens URLs
    /// </summary>
    public class HmbUrlShorteningService : UrlShorteningServiceBase
    {
        private readonly string userAgent;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HmbUrlShorteningService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="shortUrlCacheService">
        /// The short url cache repository.
        /// </param>
        /// <param name="config"></param>
        public HmbUrlShorteningService(ILogger logger,
            IShortUrlCacheService shortUrlCacheService,
            BotConfiguration config)
            : base(logger, shortUrlCacheService)
        {
            this.userAgent = config.UserAgent;
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
        protected internal override string GetShortUrl(string longUrl)
        {
            var wrq = (HttpWebRequest) WebRequest.Create(
                "https://hmb.im/shorten.php?url=" + HttpUtility.UrlEncode(longUrl));
            wrq.Timeout = 1000;
           // wrq.ContinueTimeout = 1000;
            wrq.ReadWriteTimeout = 1000;
            
            wrq.UserAgent = this.userAgent;
            var wrs = (HttpWebResponse)wrq.GetResponse();
            if (wrs.StatusCode == HttpStatusCode.OK || wrs.StatusCode == HttpStatusCode.Created)
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