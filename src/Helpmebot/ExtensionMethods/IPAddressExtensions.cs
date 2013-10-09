// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPAddressExtensions.cs" company="Helpmebot Development Team">
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
//   Defines the IPAddressExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ExtensionMethods
{
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;

    using Helpmebot.Model;

    /// <summary>
    /// The ip address extensions.
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// The get location.
        /// </summary>
        /// <param name="ip">
        /// The ip.
        /// </param>
        /// <returns>
        /// The <see cref="GeolocateResult"/>.
        /// </returns>
        public static GeolocateResult GetLocation(this IPAddress ip)
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
    }
}
