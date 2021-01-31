// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IpInfoDbGeolocationService.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.CoreServices.Services.Geolocation
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Xml;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    /// <summary>
    /// The IpInfoDB geolocation service.
    /// </summary>
    public class IpInfoDbGeolocationService : IGeolocationService
    {
        private readonly ILogger logger;
        private readonly BotConfiguration configuration;
        private readonly IWebServiceClient webServiceClient;

        public IpInfoDbGeolocationService(ILogger logger, BotConfiguration configuration, IWebServiceClient webServiceClient)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.webServiceClient = webServiceClient;
        }

        public GeolocateResult GetLocation(IPAddress address)
        {
            if (this.configuration.IpInfoDbApiKey == string.Empty)
            {
                this.logger.Error("API key is empty, please fix this in configuration.");
                return new GeolocateResult();
            }

            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            
            var queryParameters = new NameValueCollection
            {
                {"key", this.configuration.IpInfoDbApiKey},
                {"ip", address.ToString()}
            };

            using (var s = this.webServiceClient.DoApiCall(
                queryParameters,
                "https://accounts.wmflabs.org/api.php",
                this.configuration.UserAgent))
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