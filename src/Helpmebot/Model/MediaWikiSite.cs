// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaWikiSite.cs" company="Helpmebot Development Team">
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

namespace Helpmebot.Model
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    using Helpmebot.Persistence;

    /// <summary>
    /// The media wiki site.
    /// </summary>
    public class MediaWikiSite : EntityBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the API.
        /// </summary>
        public virtual string Api { get; set; }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        public virtual string Database { get; set; }

        /// <summary>
        /// Gets or sets the main page.
        /// </summary>
        public virtual string MainPage { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public virtual string Password { get; set; }

        /// <summary>
        /// Gets or sets the shard.
        /// </summary>
        public virtual string Shard { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public virtual string Username { get; set; }

        #endregion

        /// <summary>
        /// The get block information.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{BlockInformation}"/>.
        /// </returns>
        public IEnumerable<BlockInformation> GetBlockInformation(string userName)
        {
            IPAddress ip;
            var apiParams = string.Format(
                "{2}?action=query&list=blocks&bk{0}={1}&format=xml",
                IPAddress.TryParse(userName, out ip) ? "ip" : "users",
                userName,
                this.Api);

            var xmlFragment = HttpRequest.Get(apiParams);

            var xdoc = XDocument.Load(new StreamReader(xmlFragment));

            //// ReSharper disable PossibleNullReferenceException
            var blockInformations = from item in xdoc.Descendants("block")
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
                                                      AllowUserTalk =
                                                          item.Attribute("allowusertalk") != null,
                                                      AnonOnly = item.Attribute("anononly") != null
                                                  };

            //// ReSharper restore PossibleNullReferenceException
            return blockInformations;
        }
    }
}