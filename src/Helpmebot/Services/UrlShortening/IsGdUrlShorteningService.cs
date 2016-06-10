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
    using Helpmebot.Repositories.Interfaces;

    /// <summary>
    ///     Shortens URLs
    /// </summary>
    public class IsGdUrlShorteningService : UrlShorteningServiceBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IsGdUrlShorteningService"/> class.
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
        public IsGdUrlShorteningService(ILogger logger, IShortUrlCacheRepository shortUrlCacheRepository, IConfigurationHelper configurationHelper)
            : base(logger, shortUrlCacheRepository, configurationHelper)
        {
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
        protected override string GetShortUrl(string longUrl)
        {
            var wrq =
                (HttpWebRequest)
                WebRequest.Create("https://is.gd/create.php?format=simple&url=" + HttpUtility.UrlEncode(longUrl));
            wrq.UserAgent = this.ConfigurationHelper.CoreConfiguration.UserAgent;
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