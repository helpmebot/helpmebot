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
// <summary>
//   Returns the user rights of a wikipedian
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///   Returns the user rights of a wikipedian
    /// </summary>
    internal class Rights : GenericCommand
    {
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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Rights(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Gets the rights of a wikipedian.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="channel">The channel to get the base wiki for.</param>
        /// <returns>the rights</returns>
        public static string GetRights(string username, string channel)
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

            string returnStr = string.Empty;
            int rightsCount = 0;
            XmlTextReader creader =
                new XmlTextReader(
                    HttpRequest.get(api + "?action=query&list=users&usprop=groups&format=xml&ususers=" + username));
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
        
        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            CommandResponseHandler crh = new CommandResponseHandler();
            
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
            if (rights != string.Empty)
            {
                string[] messageParameters = { userName, rights };
                message = this.MessageService.RetrieveMessage("cmdRightsList", this.Channel, messageParameters);
            }
            else
            {
                string[] messageParameters = { userName };
                message = this.MessageService.RetrieveMessage("cmdRightsNone", this.Channel, messageParameters);
            }

            crh.respond(message);
            return crh;
        }
    }
}
