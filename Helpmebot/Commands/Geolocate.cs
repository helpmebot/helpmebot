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
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Xml;

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
        /// TODO: move to extension method on IPAddress
        public static GeolocateResult GetLocation(IPAddress ip)
        {
            Stream s = HttpRequest.get("http://api.ipinfodb.com/v2/ip_query.php?key=" + Configuration.singleton()["ipinfodbApiKey"] + "&ip=" + ip + "&timezone=false");
            XmlTextReader xtr = new XmlTextReader(s);
            GeolocateResult result = new GeolocateResult();

            while (!xtr.EOF)
            {
                xtr.Read();
                switch (xtr.Name)
                {
                    case "Status":
                        result.Status = xtr.ReadElementContentAsString();
                        break;
                    case "CountryCode":
                        result.CountryCode = xtr.ReadElementContentAsString();
                        break;
                    case "CountryName":
                        result.Country = xtr.ReadElementContentAsString();
                        break;
                    case "RegionCode":
                        result.RegionCode = xtr.ReadElementContentAsString();
                        break;
                    case "RegionName":
                        result.Region = xtr.ReadElementContentAsString();
                        break;
                    case "City":
                        result.City = xtr.ReadElementContentAsString();
                        break;
                    case "ZipPostalCode":
                        result.ZipPostalCode = xtr.ReadElementContentAsString();
                        break;
                    case "Latitude":
                        result.Latitude = xtr.ReadElementContentAsFloat();
                        break;
                    case "Longitude":
                        result.Longitude = xtr.ReadElementContentAsFloat();
                        break;
                }
            }

            return result;
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
                string[] messageParameters = { "geolocate", "1", this.Arguments.Length.ToString() };
                return new CommandResponseHandler(new Message().get("notEnoughParameters", messageParameters));
            }

            GeolocateResult location = GetLocation(IPAddress.Parse(this.Arguments[0]));
            string[] messageArgs = { location.ToString() };
            return new CommandResponseHandler(new Message().get("locationMessage", messageArgs));
        }

        /// <summary>
        /// Structure to hold the result of a geolocation.
        /// </summary>
        public struct GeolocateResult
        {
            /// <summary>
            /// The status.
            /// </summary>
            public string Status;

            /// <summary>
            /// The country code.
            /// </summary>
            public string CountryCode;

            /// <summary>
            /// The country.
            /// </summary>
            public string Country;

            /// <summary>
            /// The region code.
            /// </summary>
            public string RegionCode;

            /// <summary>
            /// The region.
            /// </summary>
            public string Region;

            /// <summary>
            /// The city.
            /// </summary>
            public string City;

            /// <summary>
            /// The zip postal code.
            /// </summary>
            public string ZipPostalCode;

            /// <summary>
            /// The latitude.
            /// </summary>
            public float Latitude;

            /// <summary>
            /// The longitude.
            /// </summary>
            public float Longitude;

            /// <summary>
            /// Returns a <see cref="System.String"/> that represents this geolocate result.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String"/> that represents this geolocate result.
            /// </returns>
            public override string ToString()
            {
                string estimatedLocation = string.Empty;

                List<string> locationParameters = new List<string>();
                
                if (this.City != string.Empty)
                {
                    locationParameters.Add(this.City);
                }
                
                if (this.Region != string.Empty)
                {
                    locationParameters.Add(this.Region);
                }

                if (this.Country != string.Empty)
                {
                    locationParameters.Add(this.Country);
                }

                if (locationParameters.Count > 1)
                {
                    estimatedLocation = string.Format(
                        " (Estimated location: {0})",
                        string.Join(", ", locationParameters.ToArray()));
                }

                return string.Format(
                    "Latitude: {0}N, Longitude: {1}E{2}",
                    this.Latitude,
                    this.Longitude,
                    estimatedLocation);
            }
        }
    }
}