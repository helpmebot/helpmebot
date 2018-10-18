// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaWikiSiteExtensions.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.ExtensionMethods
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using Helpmebot.Configuration;
    using Helpmebot.Model;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using HttpRequest = Helpmebot.HttpRequest;

    /// <summary>
    ///     The media wiki site extensions.
    /// </summary>
    public static class MediaWikiSiteExtensions
    {
        /// <summary>
        /// The get block information.
        /// </summary>
        /// <param name="site">
        /// The site
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{BlockInformation}"/>.
        /// </returns>
        public static IEnumerable<BlockInformation> GetBlockInformation(this MediaWikiSite site, string userName)
        {
            var wsClient = ServiceLocator.Current.GetInstance<IWebServiceClient>();
            var endpoint = site.Api;
            var userAgent = ServiceLocator.Current.GetInstance<BotConfiguration>().UserAgent;
            var cookieJar = new CookieContainer();
            
            IPAddress ip;
            string apiParams = string.Format(
                "{2}?action=query&list=blocks&bk{0}={1}&format=xml", 
                IPAddress.TryParse(userName, out ip) ? "ip" : "users", 
                userName, 
                site.Api);

            using (Stream xmlFragment = HttpRequest.Get(apiParams).ToStream())
            {
                XDocument xdoc = XDocument.Load(new StreamReader(xmlFragment));

                //// ReSharper disable PossibleNullReferenceException
                var blocks = from item in xdoc.Descendants("block")
                             select
                                 new BlockInformation
                                     {
                                         Id = item.Attribute("id").Value,
                                         Target = item.Attribute("user").Value,
                                         BlockedBy = item.Attribute("by").Value,
                                         Start = item.Attribute("timestamp").Value,
                                         Expiry = item.Attribute("expiry").Value,
                                         BlockReason = item.Attribute("reason").Value,
                                         AutoBlock = item.Attribute("autoblock") != null,
                                         NoCreate = item.Attribute("nocreate") != null,
                                         NoEmail = item.Attribute("noemail") != null,
                                         AllowUserTalk = item.Attribute("allowusertalk") != null,
                                         AnonOnly = item.Attribute("anononly") != null
                                     };

                //// ReSharper restore PossibleNullReferenceException
                return blocks;
            }
        }
    }
}