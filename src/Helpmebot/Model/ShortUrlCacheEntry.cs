// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShortUrlCacheEntry.cs" company="Helpmebot Development Team">
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
    using Helpmebot.Persistence;

    /// <summary>
    /// The short url cache entry.
    /// </summary>
    public class ShortUrlCacheEntry : EntityBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the long url.
        /// </summary>
        public virtual string LongUrl { get; set; }

        /// <summary>
        /// Gets or sets the short url.
        /// </summary>
        public virtual string ShortUrl { get; set; }

        #endregion
    }
}