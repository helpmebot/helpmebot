// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShortUrlCacheRepository.cs" company="Helpmebot Development Team">
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

namespace Helpmebot.Repositories.Interfaces
{
    using System;

    using Helpmebot.Model;

    /// <summary>
    /// The ShortUrlCacheRepository interface.
    /// </summary>
    public interface IShortUrlCacheRepository : IRepository<ShortUrlCacheEntry>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get by long url.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="ShortUrlCacheEntry"/>.
        /// </returns>
        ShortUrlCacheEntry GetByLongUrl(string url);

        #endregion

        /// <summary>
        /// The get short url.
        /// </summary>
        /// <param name="longUrl">
        /// The long url.
        /// </param>
        /// <param name="cacheMissCallback">
        /// The cache miss callback.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetShortUrl(string longUrl, Func<string, string> cacheMissCallback);
    }
}