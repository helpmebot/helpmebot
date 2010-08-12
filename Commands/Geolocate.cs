// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    /// Geolocates an IP address
    /// </summary>
    internal class Geolocate : GenericCommand
    {
        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            GeolocateResult location = getLocation(IPAddress.Parse(args[0]));
            string[] messageArgs = {location.ToString()};
            return new CommandResponseHandler(new Message().get("locationMessage", messageArgs));
        }

        /// <summary>
        /// Gets the location of the IP address.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns></returns>
        public static GeolocateResult getLocation(IPAddress ip)
        {
            Stream s = HttpRequest.get("http://ipinfodb.com/ip_query.php?timezone=false&ip=" + ip);
            XmlTextReader xtr = new XmlTextReader(s);
            GeolocateResult result = new GeolocateResult();

            while (!xtr.EOF)
            {
                xtr.Read();
                switch (xtr.Name)
                {
                    case "Status":
                        result.status = xtr.ReadElementContentAsString();
                        break;
                    case "CountryCode":
                        result.countryCode = xtr.ReadElementContentAsString();
                        break;
                    case "CountryName":
                        result.country = xtr.ReadElementContentAsString();
                        break;
                    case "RegionCode":
                        result.regionCode = xtr.ReadElementContentAsString();
                        break;
                    case "RegionName":
                        result.region = xtr.ReadElementContentAsString();
                        break;
                    case "City":
                        result.city = xtr.ReadElementContentAsString();
                        break;
                    case "ZipPostalCode":
                        result.zipPostalCode = xtr.ReadElementContentAsString();
                        break;
                    case "Latitude":
                        result.latitude = xtr.ReadElementContentAsFloat();
                        break;
                    case "Longitude":
                        result.longitude = xtr.ReadElementContentAsFloat();
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Structure to hold the result of a geolocation.
        /// </summary>
        public struct GeolocateResult
        {
            public string status;
            public string countryCode;
            public string country;
            public string regionCode;
            public string region;
            public string city;
            public string zipPostalCode;
            public float latitude;
            public float longitude;

            /// <summary>
            /// Returns a <see cref="System.String"/> that represents this geolocate result.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String"/> that represents this geolocate result.
            /// </returns>
            public override string ToString()
            {
                return this.city + ", " + this.region + ", " + this.country;
            }
        }
    }
}