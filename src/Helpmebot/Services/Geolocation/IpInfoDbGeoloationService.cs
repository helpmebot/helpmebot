// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IpInfoDbGeoloationService.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Services.Geolocation
{
    using System.IO;
    using System.Net;
    using System.Xml;

    using Castle.Core.Logging;

    using Helpmebot.Configuration.XmlSections.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The IpInfoDB geolocation service.
    /// </summary>
    public class IpInfoDbGeoloationService : IGeolocationService
    {
        /// <summary>
        /// The API key.
        /// </summary>
        private readonly string apiKey;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IpInfoDbGeoloationService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        public IpInfoDbGeoloationService(ILogger logger, IPrivateConfiguration configuration)
        {
            this.logger = logger;
            this.apiKey = configuration.IpInfoDbApiKey;
        }

        /// <summary>
        /// The get location.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <returns>
        /// The <see cref="GeolocateResult"/>.
        /// </returns>
        public GeolocateResult GetLocation(IPAddress address)
        {
            if (this.apiKey == string.Empty)
            {
                this.logger.Error("API key is empty, please fix this in configuration.");
                return new GeolocateResult();
            }

            var requestData =
                HttpRequest.Get(
                    "https://api.ipinfodb.com/v3/ip-city/?key=" + this.apiKey + "&ip=" + address + "&format=xml");

            using (Stream s = requestData.ToStream())
            {
                var xtr = new XmlTextReader(s);

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