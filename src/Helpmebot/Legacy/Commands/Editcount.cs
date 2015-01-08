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
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;
    using System.Web;
    using System.Xml.XPath;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using HttpRequest = Helpmebot.HttpRequest;

    /// <summary>
    ///     Returns the edit count of a Wikipedian
    /// </summary>
    internal class Editcount : GenericCommand
    {
        #region Constructors and Destructors

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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Editcount(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the edit count.
        /// </summary>
        /// <param name="username">
        /// The username to retrieve the edit count for.
        /// </param>
        /// <param name="channel">
        /// The channel the command was issued in. (Gets the correct base wiki)
        /// </param>
        /// <returns>
        /// The edit count
        /// </returns>
        public static int GetEditCount(string username, string channel)
        {
            if (username == string.Empty)
            {
                throw new ArgumentNullException();
            }

            string baseWiki = LegacyConfig.Singleton()["baseWiki", channel];

            // FIXME: ServiceLocator - mw site repo
            var mediaWikiSiteRepository = ServiceLocator.Current.GetInstance<IMediaWikiSiteRepository>();
            MediaWikiSite mediaWikiSite = mediaWikiSiteRepository.GetById(int.Parse(baseWiki));

            username = HttpUtility.UrlEncode(username);

            // TODO: Linq-to-XML in MediaWikiSite extension method
            var uri = string.Format(
                "{0}?format=xml&action=query&list=users&usprop=editcount&format=xml&ususers={1}",
                mediaWikiSite.Api,
                username);

            using (var data = HttpRequest.Get(uri).ToStream())
            {
                var xpd = new XPathDocument(data);

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
            }

            throw new ArgumentException();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
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
                userName = this.Source.Nickname;
            }

            int editCount = GetEditCount(userName, this.Channel);
            var messageService = this.CommandServiceHelper.MessageService;
            if (editCount == -1)
            {
                string[] messageParams = { userName };
                string message = messageService.RetrieveMessage("noSuchUser", this.Channel, messageParams);
                return new CommandResponseHandler(message);
            }
            else
            {
                string[] messageParameters = { editCount.ToString(CultureInfo.InvariantCulture), userName };

                string message = messageService.RetrieveMessage("editCount", this.Channel, messageParameters);

                return new CommandResponseHandler(message);
            }
        }

        #endregion
    }
}