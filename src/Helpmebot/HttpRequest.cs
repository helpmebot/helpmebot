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
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot
{
    using System.IO;
    using System.Net;

    using Helpmebot.Legacy.Configuration;

    /// <summary>
    /// The http request.
    /// </summary>
    internal static class HttpRequest
    {
        #region Public Methods and Operators

        /// <summary>
        /// Gets the specified URI, passing the UserAgent.
        /// </summary>
        /// <param name="uri">
        /// The URI.
        /// </param>
        /// <param name="timeout">
        /// optional. will default to httpTimeout config option 
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static Stream Get(string uri, int timeout = -1)
        {
            var hwr = (HttpWebRequest)WebRequest.Create(uri);
            hwr.UserAgent = LegacyConfig.Singleton()["useragent"];
            hwr.Timeout = timeout == -1 ? int.Parse(LegacyConfig.Singleton()["httpTimeout"]) : timeout;
            var resp = (HttpWebResponse)hwr.GetResponse();

            return resp.GetResponseStream();
        }

        #endregion
    }
}