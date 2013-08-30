﻿// --------------------------------------------------------------------------------------------------------------------
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

    /// <summary>
    /// Returns the block information of a wikipedian
    /// </summary>
    internal class Blockinfo : GenericCommand
    {
        public Blockinfo(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {

            return new CommandResponseHandler(getBlockInformation(string.Join(" ", args), channel).ToString());
        }

        /// <summary>
        /// Gets the block information for a user or IP address on a wiki.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel the command was requested in.</param>
        /// <returns></returns>
        public static BlockInformation getBlockInformation(string userName, string channel)
        {
            IPAddress ip;

            string baseWiki = Configuration.singleton()["baseWiki", channel];

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
                                          id = creader.GetAttribute("id"),
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
    }

    /// <summary>
    /// Holds the block information of a specific user
    /// </summary>
    public struct BlockInformation
    {
        public string id;

        public string target;

        public string blockedBy;

        public string blockReason;

        public string expiry;

        public string start;

        public bool nocreate;

        public bool autoblock;

        public bool noemail;

        public bool allowusertalk;

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this block.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this block.
        /// </returns>
        public override string ToString()
        {

            string[] emptyMessageParams = { "", "", "", "", "", "", "" };
            string emptyMessage = new Message().get("blockInfoShort", emptyMessageParams);

            string info = "";
            if (nocreate) info += "NOCREATE ";
            if (autoblock) info += "AUTOBLOCK ";
            if (noemail) info += "NOEMAIL ";
            if (allowusertalk) info += "ALLOWUSERTALK ";
            string[] messageParams = { id, target, blockedBy, expiry, start, blockReason, info };
            string message = new Message().get("blockInfoShort", messageParams);

            if (message == emptyMessage)
            {
                message = new Message().get("noBlocks");
            }

            return message;
        }
    }
}
