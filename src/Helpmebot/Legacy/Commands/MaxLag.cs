// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MaxLag.cs" company="Helpmebot Development Team">
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
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;

    /// <summary>
    ///     Returns the maximum replication lag on the wiki
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Maxlag : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Maxlag"/> class.
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
        public Maxlag(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
        /// </summary>
        /// <returns>The response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] messageParameters = { this.Source.Nickname, this.GetMaxLag() };
            string message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                "cmdMaxLag", 
                this.Channel, 
                messageParameters);
            return new CommandResponseHandler(message);
        }

        /// <summary>
        /// Gets the maximum replication lag between the Wikimedia Foundation MySQL database cluster for the base wiki of the
        /// channel.
        /// </summary>
        /// <returns>The maximum replication lag</returns>
        private string GetMaxLag()
        {
            // get api
            var mediaWikiSite = this.GetLocalMediawikiSite();

            // TODO: use Linq-to-XML
            var uri = mediaWikiSite.Api + "?action=query&meta=siteinfo&siprop=dbrepllag&format=xml";
            using (var data = HttpRequest.Get(uri).ToStream())
            {
                var mlreader = new XmlTextReader(data);

                do
                {
                    mlreader.Read();
                }
                while (mlreader.Name != "db");

                string lag = mlreader.GetAttribute("lag");

                return lag;
            }
        }

        #endregion
    }
}