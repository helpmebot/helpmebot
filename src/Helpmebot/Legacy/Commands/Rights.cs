// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rights.cs" company="Helpmebot Development Team">
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
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     Returns the user rights of a wikipedian
    /// </summary>
    internal class Rights : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Rights"/> class.
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
        public Rights(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the rights of a wikipedian.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="channel">
        /// The channel to get the base wiki for.
        /// </param>
        /// <returns>
        /// the rights
        /// </returns>
        public static string GetRights(string username, string channel)
        {
            if (username == string.Empty)
            {
                throw new ArgumentNullException();
            }

            string baseWiki = LegacyConfig.Singleton()["baseWiki", channel];

            // FIXME: ServiceLocator - mw site repo
            var mediaWikiSiteRepository = ServiceLocator.Current.GetInstance<IMediaWikiSiteRepository>();
            MediaWikiSite mediaWikiSite = mediaWikiSiteRepository.GetById(int.Parse(baseWiki));

            string returnStr = string.Empty;
            int rightsCount = 0;
            var creader =
                new XmlTextReader(
                    HttpRequest.Get(
                        mediaWikiSite.Api + "?action=query&list=users&usprop=groups&format=xml&ususers=" + username));
            do
            {
                creader.Read();
            }
            while (creader.Name != "user");

            creader.Read();
            if (creader.Name == "groups")
            {
                // the start of the group list
                do
                {
                    creader.Read();
                    string rightsList = creader.ReadString();
                    if (!(rightsList == string.Empty || rightsList == "*"))
                    {
                        returnStr = returnStr + rightsList + ", ";
                    }

                    rightsCount = rightsCount + 1;
                }
                while (creader.Name == "g"); // each group should be added
            }

            returnStr = rightsCount == 0 ? string.Empty : returnStr.Remove(returnStr.Length - 2);

            return returnStr;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var crh = new CommandResponseHandler();

            string userName;
            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                userName = string.Join(" ", this.Arguments);
            }
            else
            {
                userName = this.Source.Nickname;
            }

            string rights = GetRights(userName, this.Channel);

            string message;
            var messageService = this.CommandServiceHelper.MessageService;
            if (rights != string.Empty)
            {
                string[] messageParameters = { userName, rights };
                message = messageService.RetrieveMessage("cmdRightsList", this.Channel, messageParameters);
            }
            else
            {
                string[] messageParameters = { userName };
                message = messageService.RetrieveMessage("cmdRightsNone", this.Channel, messageParameters);
            }

            crh.Respond(message);
            return crh;
        }

        #endregion
    }
}