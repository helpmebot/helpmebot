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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    using Helpmebot.Model;

    /// <summary>
    ///     The media wiki site extensions.
    /// </summary>
    public static class MediaWikiSiteExtensions
    {
        #region Public Methods and Operators

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

        /// <summary>
        /// The get category size.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified page is not a category.
        /// </exception>
        public static int GetCategorySize(this MediaWikiSite site, string category)
        {
            string apiCall = string.Format(
                "{1}?action=query&format=xml&prop=categoryinfo&titles=Category:{0}", 
                category, 
                site.Api);

            using (Stream xmlFragment = HttpRequest.Get(apiCall).ToStream())
            {
                var xdoc = XDocument.Load(new StreamReader(xmlFragment));

                var countEnumerable = from item in xdoc.Descendants("categoryinfo")
                                      let xAttribute = item.Attribute("pages")
                                      where xAttribute != null
                                      select int.Parse(xAttribute.Value);

                var countList = countEnumerable.ToList();

                if (!countList.Any())
                {
                    throw new ArgumentException("Category does not exist!");
                }

                return countList.FirstOrDefault();
            }
        }

        /// <summary>
        /// The get pages in category.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <returns>
        /// The <see cref="List{String}"/>.
        /// </returns>
        public static List<string> GetPagesInCategory(this MediaWikiSite site, string category)
        {
            string uri = site.Api + "?action=query&list=categorymembers&format=xml&cmlimit=50&cmprop=title&cmtitle="
                         + category;

            using (Stream xmlFragment = HttpRequest.Get(uri).ToStream())
            {
                XDocument xdoc = XDocument.Load(new StreamReader(xmlFragment));

                IEnumerable<string> pages = from item in xdoc.Descendants("cm")
                                            let xAttribute = item.Attribute("title")
                                            where xAttribute != null
                                            select xAttribute.Value;

                return pages.ToList();
            }
        }

        #endregion
    }
}