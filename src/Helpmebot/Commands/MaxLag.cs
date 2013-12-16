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
// <summary>
//   Returns the maximum replication lag on the wiki
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///   Returns the maximum replication lag on the wiki
    /// </summary>
    internal class Maxlag : GenericCommand
    {
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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Maxlag(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Gets the maximum replication lag between the Wikimedia Foundation MySQL database cluster for the base wiki of the channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <returns>The maximum replication lag</returns>
        public static string GetMaxLag(string channel)
        {
            // look up site id
            string baseWiki = LegacyConfig.singleton()["baseWiki", channel];
             
            // get api
            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            XmlTextReader mlreader =
                new XmlTextReader(HttpRequest.get(api + "?action=query&meta=siteinfo&siprop=dbrepllag&format=xml"));
            do
            {
                mlreader.Read();
            }
            while (mlreader.Name != "db");

            string lag = mlreader.GetAttribute("lag");

            return lag;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] messageParameters = { this.Source.nickname, GetMaxLag(this.Channel) };
            string message = this.MessageService.RetrieveMessage("cmdMaxLag",this.Channel, messageParameters);
            return new CommandResponseHandler(message);
        }
    }
}
