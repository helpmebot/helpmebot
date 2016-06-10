// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GooglUrlShorteningService.cs" company="Helpmebot Development Team">
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
    using System;
    using System.IO;
    using System.Net;

    using Castle.Core.Logging;

    using Helpmebot.Configuration;
    using Helpmebot.Repositories.Interfaces;

    using Newtonsoft.Json;

    /// <summary>
    /// The http://goo.gl/ url shortening service.
    /// </summary>
    public class GooglUrlShorteningService : UrlShorteningServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GooglUrlShorteningService"/> class.
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
        public GooglUrlShorteningService(
            ILogger logger, 
            IShortUrlCacheRepository shortUrlCacheRepository, 
            IConfigurationHelper configurationHelper)
            : base(logger, shortUrlCacheRepository, configurationHelper)
        {
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
        protected override string GetShortUrl(string longUrl)
        {
            var googleApiKey = this.ConfigurationHelper.PrivateConfiguration.GoogleApiKey;
            var url = string.Format("https://www.googleapis.com/urlshortener/v1/url?key={0}", googleApiKey);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";

            var content = new { longUrl };

            var streamWriter = new StreamWriter(request.GetRequestStream());
            streamWriter.Write(JsonConvert.SerializeObject(content));
            streamWriter.Flush();

            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
            {
                throw new NullReferenceException("Null response stream recieved");
            }

            var streamReader = new StreamReader(responseStream);
            dynamic responseContent = JsonConvert.DeserializeObject(streamReader.ReadToEnd());

            return responseContent.id;
        }
    }
}