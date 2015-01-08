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
    using System.Xml;

    using Castle.Core.Logging;

    using Helpmebot.Configuration.XmlSections.Interfaces;
    using Helpmebot.Model;

    using Microsoft.Practices.ServiceLocation;

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
            // FIXME: ServiceLocator - logger & privateconfig
            var config = ServiceLocator.Current.GetInstance<IPrivateConfiguration>();
            var logger = ServiceLocator.Current.GetInstance<ILogger>().CreateChildLogger("IPAddressExtensions");

            if (config.IpInfoDbApiKey == string.Empty)
            {
                logger.Error("API key is empty, please fix this in configuration.");
                return new GeolocateResult();
            }

            var requestData =
                HttpRequest.Get(
                    "http://api.ipinfodb.com/v3/ip-city/?key=" + config.IpInfoDbApiKey + "&ip=" + ip + "&format=xml");

            using (Stream s = requestData.ToStream())
            {
                using (var xtr = new XmlTextReader(s))
                {
                    var result = new GeolocateResult();

                    while (!xtr.EOF)
                    {
                        xtr.Read();
                        switch (xtr.Name)
                        {
                            case "statusCode":
                                result.Status = xtr.ReadElementContentAsString();
                                break;
                            case "countryCode":
                                result.CountryCode = xtr.ReadElementContentAsString();
                                break;
                            case "countryName":
                                result.Country = xtr.ReadElementContentAsString();
                                break;
                            case "regionName":
                                result.Region = xtr.ReadElementContentAsString();
                                break;
                            case "cityName":
                                result.City = xtr.ReadElementContentAsString();
                                break;
                            case "zipCode":
                                result.ZipPostalCode = xtr.ReadElementContentAsString();
                                break;
                            case "latitude":
                                result.Latitude = xtr.ReadElementContentAsFloat();
                                break;
                            case "longitude":
                                result.Longitude = xtr.ReadElementContentAsFloat();
                                break;
                        }
                    }

                    return result;
                }
            }
        }
    }
}
