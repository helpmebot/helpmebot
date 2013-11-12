// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Editcount.cs" company="Helpmebot Development Team">
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
//   Returns the edit count of a wikipedian
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Web;
    using System.Xml.XPath;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Services.Interfaces;

    using HttpRequest = Helpmebot.HttpRequest;

    /// <summary>
    ///   Returns the edit count of a Wikipedian
    /// </summary>
    internal class Editcount : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Editcount"/> class.
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
        public Editcount(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Gets the edit count.
        /// </summary>
        /// <param name="username">The username to retrieve the edit count for.</param>
        /// <param name="channel">The channel the command was issued in. (Gets the correct base wiki)</param>
        /// <returns>The edit count</returns>
        public static int GetEditCount(string username, string channel)
        {
            if (username == string.Empty)
            {
                throw new ArgumentNullException();
            }

            string baseWiki = LegacyConfig.singleton()["baseWiki", channel];

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            username = HttpUtility.UrlEncode(username);

            XPathDocument xpd =
                new XPathDocument(
                    HttpRequest.get(
                        api + "?format=xml&action=query&list=users&usprop=editcount&format=xml&ususers=" + username));

            XPathNodeIterator xpni = xpd.CreateNavigator().Select("//user");

            if (xpni.MoveNext())
            {
                string editcount = xpni.Current.GetAttribute("editcount", string.Empty);
                if (editcount != string.Empty)
                {
                    return int.Parse(editcount);
                }

                if (xpni.Current.GetAttribute("missing", string.Empty) == string.Empty)
                {
                    // TODO: uint? rather than -1
                    return -1;
                }
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Actual command logic    
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string userName;
            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                userName = string.Join(" ", this.Arguments);
            }
            else
            {
                userName = this.Source.nickname;
            }

            int editCount = GetEditCount(userName, this.Channel);
            if (editCount == -1)
            {
                string[] messageParams = { userName };
                string message = new Message().GetMessage("noSuchUser", messageParams);
                return new CommandResponseHandler(message);
            }
            else
            {
                string[] messageParameters = { editCount.ToString(), userName };

                string message = new Message().GetMessage("editCount", messageParameters);

                return new CommandResponseHandler(message);
            }
        }
    }
}