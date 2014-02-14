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
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     Returns the registration date of a wikipedian
    /// </summary>
    internal class Registration : GenericCommand
    {
        #region Static Fields

        /// <summary>
        ///     The registration cache.
        /// </summary>
        private static readonly Dictionary<string, DateTime> RegistrationCache = new Dictionary<string, DateTime>();

        #endregion

        #region Constructors and Destructors

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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Registration(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Public Methods and Operators

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

            string baseWiki = LegacyConfig.Singleton()["baseWiki", channel];

            if (RegistrationCache.ContainsKey(baseWiki + "||" + username))
            {
                return RegistrationCache[baseWiki + "||" + username];
            }

            // FIXME: ServiceLocator
            var mediaWikiSiteRepository = ServiceLocator.Current.GetInstance<IMediaWikiSiteRepository>();
            MediaWikiSite mediaWikiSite = mediaWikiSiteRepository.GetById(int.Parse(baseWiki));

            var creader =
                new XmlTextReader(
                    HttpRequest.Get(
                        mediaWikiSite.Api + "?action=query&list=users&usprop=registration&format=xml&ususers="
                        + username));
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
                    RegistrationCache.Add(baseWiki + "||" + username, registrationDate);
                    return registrationDate;
                }

                DateTime regDate = DateTime.Parse(apiRegDate);
                RegistrationCache.Add(baseWiki + "||" + username, regDate);
                return regDate;
            }

            return new DateTime(0);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The execute command.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            // FIXME: servicelocator
            var ircClient = ServiceLocator.Current.GetInstance<IIrcClient>();

            var crh = new CommandResponseHandler();
            var messageService = this.CommandServiceHelper.MessageService;
            if (this.Arguments.Length > 0)
            {
                string userName = string.Join(" ", this.Arguments);
                DateTime registrationDate = GetRegistrationDate(userName, this.Channel);
                if (registrationDate == new DateTime(0))
                {
                    string[] messageParams = { userName };
                    string message = messageService.RetrieveMessage("noSuchUser", this.Channel, messageParams);
                    crh.Respond(message);
                }
                else
                {
                    string[] messageParameters =
                        {
                            userName, registrationDate.ToString("hh:mm:ss t"), 
                            registrationDate.ToString("d MMMM yyyy")
                        };
                    string message = messageService.RetrieveMessage(
                        "registrationDate", 
                        this.Channel, 
                        messageParameters);
                    crh.Respond(message);
                }
            }
            else
            {
                string[] messageParameters =
                    {
                        "registration", "1", 
                        this.Arguments.Length.ToString(CultureInfo.InvariantCulture)
                    };

                string notEnoughParamsMessage = messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters);
                ircClient.SendNotice(this.Source.Nickname, notEnoughParamsMessage);
            }

            return crh;
        }

        #endregion
    }
}