// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Geolocate.cs" company="Helpmebot Development Team">
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
//   Discovers the location of an IP address
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Globalization;
    using System.Net;

    using Helpmebot;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Model;

    using NHibernate.Param;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// Discovers the location of an IP address
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Advanced)]
    internal class Geolocate : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Geolocate"/> class.
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
        public Geolocate(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>
        /// The response
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var messageService = this.CommandServiceHelper.MessageService;
            if (this.Arguments.Length == 0)
            {
                string[] messageParameters = { "geolocate", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture) };
                return new CommandResponseHandler(messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            IPAddress address;
            if (IPAddress.TryParse(this.Arguments[0], out address))
            {
                GeolocateResult location = address.GetLocation();
                string[] messageArgs = { location.ToString() };
                return new CommandResponseHandler(messageService.RetrieveMessage("locationMessage", this.Channel, messageArgs));
            }

            return new CommandResponseHandler(messageService.RetrieveMessage("BadIpAddress", this.Channel, new string[0]));
        }
    }
}
