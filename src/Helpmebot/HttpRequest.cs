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
    using System;
    using System.IO;
    using System.Net;

    using Helpmebot.Configuration;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     The http request.
    /// </summary>
    internal static class HttpRequest
    {
        #region Static Fields

        /// <summary>
        ///     The configuration helper.
        /// </summary>
        private static IConfigurationHelper configurationHelper;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the specified URI, passing the UserAgent.
        /// </summary>
        /// <param name="uri">
        /// The URI.
        /// </param>
        /// <param name="timeout">
        /// optional. will default to httpTimeout XML config option
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static string Get(string uri, int timeout = -1)
        {
            if (configurationHelper == null)
            {
                configurationHelper = ServiceLocator.Current.GetInstance<IConfigurationHelper>();
            }

            var hwr = (HttpWebRequest)WebRequest.Create(uri);
            hwr.UserAgent = configurationHelper.CoreConfiguration.UserAgent;
            hwr.Timeout = timeout == -1 ? configurationHelper.CoreConfiguration.HttpTimeout : timeout;

            string data;

            using (var resp = (HttpWebResponse)hwr.GetResponse())
            {
                Stream responseStream = resp.GetResponseStream();

                if (responseStream == null)
                {
                    throw new NullReferenceException("Returned web request response stream was null.");
                }

                var streamReader = new StreamReader(responseStream);
                data = streamReader.ReadToEnd();

                resp.Close();
            }

            return data;
        }

        #endregion
    }
}