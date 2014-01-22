// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockInfo.cs" company="Helpmebot Development Team">
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
namespace helpmebot6.Commands
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     Returns the block information of a wikipedian
    /// </summary>
    internal class Blockinfo : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Blockinfo"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Blockinfo(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the block information for a user or IP address on a wiki.
        /// </summary>
        /// <param name="userName">
        /// Name of the user.
        /// </param>
        /// <param name="channel">
        /// The channel the command was requested in.
        /// </param>
        /// <returns>
        /// the block information
        /// </returns>
        public static BlockInformation GetBlockInformation(string userName, string channel)
        {
            IPAddress ip;

            string baseWiki = LegacyConfig.Singleton()["baseWiki", channel];

            // FIXME: ServiceLocator
            var mediaWikiSiteRepository = ServiceLocator.Current.GetInstance<IMediaWikiSiteRepository>();
            MediaWikiSite mediaWikiSite = mediaWikiSiteRepository.GetById(int.Parse(baseWiki));

            string apiParams = string.Format(
                "{2}?action=query&list=blocks&bk{0}={1}&format=xml", 
                IPAddress.TryParse(userName, out ip) ? "ip" : "users", 
                userName, 
                mediaWikiSite.Api);

            Stream xmlFragment = HttpRequest.Get(apiParams);

            XDocument xdoc = XDocument.Load(new StreamReader(xmlFragment));

            //// ReSharper disable PossibleNullReferenceException
            IEnumerable<BlockInformation> x = from item in xdoc.Descendants("block")
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
            return x.FirstOrDefault();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            return
                new CommandResponseHandler(
                    GetBlockInformation(string.Join(" ", this.Arguments), this.Channel).ToString());
        }

        #endregion
    }
}