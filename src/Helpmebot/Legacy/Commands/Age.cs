// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Age.cs" company="Helpmebot Development Team">
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

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    ///     Returns the age of a wikipedian
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Age : GenericCommand
    {
        public Age(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

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
            
            var messageService = this.CommandServiceHelper.MessageService;
            var site = this.GetLocalMediawikiSite();

            try
            {
                var registrationDate = site.GetRegistrationDate(userName);

                if (!registrationDate.HasValue)
                {
                    return new CommandResponseHandler(
                        "Cannot calculate age - no registration date found for the specified user");
                }

                var time = DateTime.Now.Subtract(registrationDate.Value);
                
                string[] messageParameters =
                {
                    userName, (time.Days / 365).ToString(CultureInfo.InvariantCulture),
                    (time.Days % 365).ToString(CultureInfo.InvariantCulture),
                    time.Hours.ToString(CultureInfo.InvariantCulture),
                    time.Minutes.ToString(CultureInfo.InvariantCulture),
                    time.Seconds.ToString(CultureInfo.InvariantCulture)
                };
                
                var message = messageService.RetrieveMessage("cmdAge", this.Channel, messageParameters);
                
                return new CommandResponseHandler(message);
            }
            catch (MediawikiApiException)
            {
                var message = messageService.RetrieveMessage("noSuchUser", this.Channel, new[] {userName});
                return new CommandResponseHandler(message);
            }
        }
    }
}