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
    using System.Globalization;
    using Helpmebot;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    ///     Returns the registration date of a wikipedian
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Registration : GenericCommand
    {
        public Registration(
            IUser source,
            string channel,
            string[] args,
            ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        ///     The execute command.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var crh = new CommandResponseHandler();
            var messageService = this.CommandServiceHelper.MessageService;
            var site = this.GetLocalMediawikiSite();

            if (this.Arguments.Length == 0)
            {
                string[] messageParameters =
                {
                    "registration", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture)
                };

                var notEnoughParamsMessage = messageService.RetrieveMessage(
                    Messages.NotEnoughParameters,
                    this.Channel,
                    messageParameters);

                this.CommandServiceHelper.Client.SendNotice(this.Source.Nickname, notEnoughParamsMessage);

                return crh;
            }

            var userName = string.Join(" ", this.Arguments);
            
            
            try
            {
                var registrationDate = site.GetRegistrationDate(userName);

                if (!registrationDate.HasValue)
                {
                    return new CommandResponseHandler( "No registration date found for the specified user");
                }

                string[] messageParameters =
                {
                    userName, registrationDate.Value.ToString("hh:mm:ss t"),
                    registrationDate.Value.ToString("d MMMM yyyy")
                };

                var message = messageService.RetrieveMessage("registrationDate", this.Channel, messageParameters);
                crh.Respond(message);
            }
            catch (MediawikiApiException)
            {
                string[] messageParams = {userName};
                string message = messageService.RetrieveMessage("noSuchUser", this.Channel, messageParams);
                crh.Respond(message);
            }

            return crh;
        }
    }
}