// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeolocateResult.cs" company="Helpmebot Development Team">
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
//   Structure to hold the result of a geolocation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Structure to hold the result of a geolocation.
    /// </summary>
    public class GeolocateResult
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the zip postal code.
        /// </summary>
        public string ZipPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        public double? Longitude { get; set; }

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

            if (this.Latitude.HasValue && this.Longitude.HasValue)
            {
                return string.Format(
                    "Latitude: {0}N, Longitude: {1}E{2}",
                    this.Latitude.Value,
                    this.Longitude.Value,
                    estimatedLocation);
            }

            return estimatedLocation;
        }
    }
}
