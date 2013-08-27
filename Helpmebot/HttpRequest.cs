// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpRequest.cs" company="Helpmebot Development Team">
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
//   Defines the HttpRequest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6
{
    using System.IO;
    using System.Net;

    internal static class HttpRequest
    {
        /// <summary>
        /// Gets the specified URI, passing the UserAgent.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="timeout">optional. will default to httpTimeout config option </param>
        /// <returns></returns>
        public static Stream get(string uri, int timeout = -1)
        {
            HttpWebRequest hwr = (HttpWebRequest) WebRequest.Create(uri);
            hwr.UserAgent = Configuration.singleton()["useragent"];
            hwr.Timeout = timeout == -1 ? int.Parse(Configuration.singleton()["httpTimeout"]) : timeout;
            HttpWebResponse resp = (HttpWebResponse) hwr.GetResponse();

            return resp.GetResponseStream();
        }
    }
}