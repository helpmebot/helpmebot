// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MaxMindGeolocationService.cs" company="Helpmebot Development Team">
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
    using System;
    using System.Net;

    using Castle.Core.Logging;

    using Helpmebot.Configuration.XmlSections.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using MaxMind.GeoIP2;
    using MaxMind.GeoIP2.Responses;

    /// <summary>
    /// The max mind geolocation service.
    /// </summary>
    public class MaxMindGeolocationService : IGeolocationService, IDisposable
    {
        /// <summary>
        /// The database path.
        /// </summary>
        private readonly string databasePath;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The reader.
        /// </summary>
        private DatabaseReader reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxMindGeolocationService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        public MaxMindGeolocationService(ILogger logger, IPrivateConfiguration configuration)
        {
            this.logger = logger;
            this.databasePath = configuration.MaxMindDatabasePath;

            var file = this.databasePath + "/GeoLite2-City.mmdb";
            this.reader = new DatabaseReader(file);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
            if (this.databasePath == string.Empty || this.reader == null)
            {
                this.logger.Error("Database path is empty, please fix this in configuration.");
                return new GeolocateResult();
            }

            lock (this.reader)
            {
                CityResponse response;
                if (!this.reader.TryCity(address, out response))
                {
                    return new GeolocateResult();
                }

                return new GeolocateResult
                           {
                               City = response.City.Name, 
                               Country = response.Country.Name, 
                               CountryCode = response.Country.IsoCode, 
                               Latitude = response.Location.Latitude, 
                               Longitude = response.Location.Longitude, 
                               Region = response.MostSpecificSubdivision.Name, 
                               ZipPostalCode = response.Postal.Code, 
                           };
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.reader != null)
                {
                    lock (this.reader)
                    {
                        this.reader.Dispose();
                        this.reader = null;
                    }
                }
            }
        }
    }
}