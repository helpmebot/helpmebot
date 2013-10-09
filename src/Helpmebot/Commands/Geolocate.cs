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
    using System;
    using System.Globalization;
    using System.Net;

    using Helpmebot;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;

    /// <summary>
    /// Discovers the location of an IP address
    /// </summary>
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
        public Geolocate(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Gets the location of the IP address.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns>The location of the address</returns>
        [Obsolete("Use extension method.")]
        public static GeolocateResult GetLocation(IPAddress ip)
        {
            return ip.GetLocation();
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>
        /// The response
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length == 0)
            {
                string[] messageParameters = { "geolocate", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture) };
                return new CommandResponseHandler(new Message().get("notEnoughParameters", messageParameters));
            }

            GeolocateResult location = IPAddress.Parse(this.Arguments[0]).GetLocation();
            string[] messageArgs = { location.ToString() };
            return new CommandResponseHandler(new Message().get("locationMessage", messageArgs));
        }
    }
}