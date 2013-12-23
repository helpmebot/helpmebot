// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Registration.cs" company="Helpmebot Development Team">
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
//   Returns the registration date of a wikipedian
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///   Returns the registration date of a wikipedian
    /// </summary>
    internal class Registration : GenericCommand
    {
        /// <summary>
        /// The registration cache.
        /// </summary>
        private static Dictionary<string, DateTime> registrationCache =
            new Dictionary<string, DateTime>();

        /// <summary>
        /// Initialises a new instance of the <see cref="Registration"/> class.
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
        public Registration(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// The get registration date.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// if username is empty;
        /// </exception>
        public static DateTime GetRegistrationDate(string username, string channel)
        {
            if (username == string.Empty)
            {
                throw new ArgumentNullException();
            }

            string baseWiki = LegacyConfig.singleton()["baseWiki", channel];

            if (registrationCache.ContainsKey(baseWiki + "||" + username))
            {
                return registrationCache[baseWiki + "||" + username];
            }

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);
            XmlTextReader creader =
                new XmlTextReader(
                    HttpRequest.get(api + "?action=query&list=users&usprop=registration&format=xml&ususers=" + username));
            do
            {
                creader.Read();
            }
            while (creader.Name != "user");

            string apiRegDate = creader.GetAttribute("registration");
            if (apiRegDate != null)
            {
                if (apiRegDate == string.Empty)
                {
                    var registrationDate = new DateTime(1970, 1, 1, 0, 0, 0);
                    registrationCache.Add(baseWiki + "||" + username, registrationDate);
                    return registrationDate;
                }

                DateTime regDate = DateTime.Parse(apiRegDate);
                registrationCache.Add(baseWiki + "||" + username, regDate);
                return regDate;
            }

            return new DateTime(0);
        }

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            CommandResponseHandler crh = new CommandResponseHandler();
            if (this.Arguments.Length > 0)
            {
                string userName = string.Join(" ", this.Arguments);
                DateTime registrationDate = GetRegistrationDate(userName, this.Channel);
                if (registrationDate == new DateTime(0))
                {
                    string[] messageParams = { userName };
                    string message = this.MessageService.RetrieveMessage("noSuchUser", this.Channel, messageParams);
                    crh.respond(message);
                }
                else
                {
                    string[] messageParameters =
                        {
                            userName, registrationDate.ToString("hh:mm:ss t"),
                            registrationDate.ToString("d MMMM yyyy")
                        };
                    string message = this.MessageService.RetrieveMessage("registrationDate", this.Channel, messageParameters);
                    crh.respond(message);
                }
            }
            else
            {
                string[] messageParameters = { "registration", "1", this.Arguments.Length.ToString() };
                Helpmebot6.irc.IrcNotice(
                    this.Source.nickname,
                    this.MessageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            return crh;
        }
    }
}
