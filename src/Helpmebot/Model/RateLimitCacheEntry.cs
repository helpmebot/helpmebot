// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RateLimitCacheEntry.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Model
{
    using System;

    /// <summary>
    /// The rate limit cache entry.
    /// </summary>
    public class RateLimitCacheEntry
    {
        /// <summary>
        /// Gets or sets the counter.
        /// </summary>
        public int Counter { get; set; }

        /// <summary>
        /// Gets or sets the expiry.
        /// </summary>
        public DateTime Expiry { get; set; }
    }
}