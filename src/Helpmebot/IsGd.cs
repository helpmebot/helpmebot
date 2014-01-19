// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsGd.cs" company="Helpmebot Development Team">
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
namespace Helpmebot
{
    using System;
    using System.IO;
    using System.Net;

    using Castle.Core.Logging;

    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     http://is.gd wrapper class
    /// </summary>
    internal class IsGd
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Shortens the specified long URL.
        /// </summary>
        /// <param name="longUrl">
        /// The long URL.
        /// </param>
        /// <returns>
        /// The <see cref="Uri"/>.
        /// </returns>
        public static Uri Shorten(Uri longUrl)
        {
            var q = new LegacyDatabase.Select("suc_shorturl");
            q.SetFrom("shorturlcache");
            q.AddWhere(new LegacyDatabase.WhereConds("suc_fullurl", longUrl.ToString()));
            string cachelookup = LegacyDatabase.Singleton().ExecuteScalarSelect(q);

            if (cachelookup == string.Empty)
            {
                try
                {
                    string shorturl = GetShortUrl(longUrl);
                    LegacyDatabase.Singleton().Insert("shorturlcache", string.Empty, longUrl.ToString(), shorturl);
                    return new Uri(shorturl);
                }
                catch (WebException ex)
                {
                    ServiceLocator.Current.GetInstance<ILogger>().Error(ex.Message, ex);
                    return longUrl;
                }
            }

            return new Uri(cachelookup);
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
        private static string GetShortUrl(Uri longUrl)
        {
            var wrq = (HttpWebRequest)WebRequest.Create("http://is.gd/api.php?longurl=" + longUrl);
            wrq.UserAgent = LegacyConfig.Singleton()["useragent"];
            var wrs = (HttpWebResponse)wrq.GetResponse();
            if (wrs.StatusCode == HttpStatusCode.OK)
            {
                var sr = new StreamReader(wrs.GetResponseStream());
                string shorturl = sr.ReadLine();
                return shorturl;
            }

            throw new WebException();
        }

        #endregion
    }
}