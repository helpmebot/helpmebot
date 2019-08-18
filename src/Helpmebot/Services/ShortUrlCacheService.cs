// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShortUrlCacheRepository.cs" company="Helpmebot Development Team">
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

namespace Helpmebot.Repositories
{
    using System;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;

    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using NHibernate.Criterion;

    public class ShortUrlCacheService : IShortUrlCacheService
    {
        private readonly ISession session;
        private readonly ILogger logger;

        public ShortUrlCacheService(ISession session, ILogger logger)
        {
            this.session = session;
            this.logger = logger;
        }

        public string GetShortUrl(string longUrl, Func<string, string> cacheMissCallback)
        {
            var result = longUrl;
            var tx = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                this.logger.DebugFormat("Searching cache for {0}", longUrl);
                var shortUrlCacheEntry = this.session.CreateCriteria<ShortUrlCacheEntry>()
                    .Add(Restrictions.Eq("LongUrl", longUrl))
                    .List<ShortUrlCacheEntry>()
                    .FirstOrDefault();

                if (shortUrlCacheEntry == null)
                {
                    this.logger.DebugFormat("Cache MISS for {0}", longUrl);

                    var shortUrl = cacheMissCallback(longUrl);

                    shortUrlCacheEntry = new ShortUrlCacheEntry {LongUrl = longUrl, ShortUrl = shortUrl};
                    this.session.SaveOrUpdate(shortUrlCacheEntry);
                    result = shortUrlCacheEntry.ShortUrl;
                }
                else
                {
                    this.logger.DebugFormat("Cache HIT for {0}", longUrl);
                    result = shortUrlCacheEntry.ShortUrl;
                }
                
                tx.Commit();
            }
            catch (Exception e)
            {
                tx.Rollback();
                this.logger.Error("Error encountered resolving URL", e);
            }
            finally
            {
                if (tx.IsActive)
                {
                    tx.Rollback();
                }
            }

            return result;
        }
    }
}