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
// <summary>
//   Returns the block information of a wikipedian
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Net;
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// Returns the block information of a wikipedian
    /// </summary>
    internal class Blockinfo : GenericCommand
    {
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

        /// <summary>
        /// Gets the block information for a user or IP address on a wiki.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel the command was requested in.</param>
        /// <returns>the block information</returns>
        public static BlockInformation GetBlockInformation(string userName, string channel)
        {
            IPAddress ip;

            string baseWiki = LegacyConfig.singleton()["baseWiki", channel];

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            string apiParams = "?action=query&list=blocks&bk";
            if (IPAddress.TryParse(userName, out ip))
            {
                apiParams += "ip";
            }
            else
            {
                apiParams += "users";
            }
            apiParams += "=" + userName + "&format=xml";
            XmlTextReader creader = new XmlTextReader(HttpRequest.get(api + apiParams));

            while (creader.Name != "blocks")
            {
                creader.Read();
            }
            creader.Read();

            if (creader.Name != "block")
            {
                return new BlockInformation();
            }

            BlockInformation bi = new BlockInformation
                                      {
                                          Id = creader.GetAttribute("id"),
                                          target = creader.GetAttribute("user"),
                                          blockedBy = creader.GetAttribute("by"),
                                          start = creader.GetAttribute("timestamp"),
                                          expiry = creader.GetAttribute("expiry"),
                                          blockReason = creader.GetAttribute("reason"),
                                          autoblock = creader.GetAttribute("autoblock") == string.Empty,
                                          nocreate = creader.GetAttribute("nocreate") == string.Empty,
                                          noemail = creader.GetAttribute("noemail") == string.Empty,
                                          allowusertalk =
                                              creader.GetAttribute("allowusertalk") == string.Empty
                                      };

            return bi;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            return new CommandResponseHandler(GetBlockInformation(string.Join(" ", this.Arguments), this.Channel).ToString());
        }
    }
}
