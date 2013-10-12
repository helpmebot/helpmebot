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
// <summary>
//   is.gd wrapper class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;

    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;

    using log4net;

    /// <summary>
    /// is.gd wrapper class
    /// </summary>
    internal class IsGd
    {
        /// <summary>
        /// The log4net logger for this class
        /// </summary>
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Shortens the specified long URL.
        /// </summary>
        /// <param name="longUrl">The long URL.</param>
        /// <returns></returns>
        public static Uri shorten(Uri longUrl)
        {
            DAL.Select q = new DAL.Select("suc_shorturl");
            q.setFrom("shorturlcache");
            q.addWhere(new DAL.WhereConds("suc_fullurl", longUrl.ToString()));
            string cachelookup = DAL.singleton().executeScalarSelect(q);

            if (cachelookup == "")
            {
                try
                {
                    string shorturl = getShortUrl(longUrl);
                    DAL.singleton().insert("shorturlcache", "", longUrl.ToString(), shorturl);
                    return new Uri(shorturl);
                }
                catch(WebException ex)
                {
                    Log.Error(ex.Message, ex);
                    return longUrl;
                }
            }

            return new Uri(cachelookup);
        }

        private static string getShortUrl(Uri longUrl)
        {
            HttpWebRequest wrq = (HttpWebRequest) WebRequest.Create("http://is.gd/api.php?longurl=" + longUrl);
            wrq.UserAgent = LegacyConfig.singleton()["useragent"];
            HttpWebResponse wrs = (HttpWebResponse) wrq.GetResponse();
            if (wrs.StatusCode == HttpStatusCode.OK)
            {
                StreamReader sr = new StreamReader(wrs.GetResponseStream());
                string shorturl = sr.ReadLine();
                return shorturl;
            }
            
            throw new WebException();
        }
    }
}