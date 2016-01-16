// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.cs" company="Helpmebot Development Team">
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
namespace helpmebot6.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;

    using RateLimitCacheEntry = NHibernate.Linq.Tuple<System.DateTime, int>;

    /// <summary>
    ///     Triggers an inter-channel alert
    /// </summary>
    internal class Helper : GenericCommand
    {
        /// <summary>
        /// The rate limit max.
        /// </summary>
        /// <remarks>
        /// TODO: push this into a config option
        /// </remarks>
        private const int RateLimitMax = 2;

        /// <summary>
        /// The rate limit duration in minutes
        /// </summary>
        /// <remarks>
        /// TODO: push this into a config option
        /// </remarks>
        private const int RateLimitDuration = 5;

        /// <summary>
        /// The rate limit cache.
        /// </summary>
        private static readonly Dictionary<string, RateLimitCacheEntry> RateLimitCache =
            new Dictionary<string, RateLimitCacheEntry>();

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Helper"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Helper(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            // TODO: this needs putting into its own subsystem, messageifying, configifying, etc.
            if (this.Channel == "#wikipedia-en-help")
            {
                if (this.RateLimit())
                {
                    return null;
                }

                string message = "[HELP]: " + this.Source + " needs help in #wikipedia-en-help !";
                if (this.Arguments.Length > 0)
                {
                    message += " (message: \"" + string.Join(" ", this.Arguments) + "\")";
                }

                this.CommandServiceHelper.Client.SendNotice("#wikipedia-en-helpers", message);
            }

            return null;
        }

        /// <summary>
        /// The rate limit.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool RateLimit()
        {
            // TODO: rate limiting needs to be tidyed up a bit
            lock (RateLimitCache)
            {
                if (RateLimitCache.ContainsKey(this.Source.Hostname))
                {
                    this.Log.Debug("Rate limit key found.");

                    var cacheEntry = RateLimitCache[this.Source.Hostname];

                    if (cacheEntry.First.AddMinutes(RateLimitDuration) >= DateTime.Now)
                    {
                        this.Log.Debug("Rate limit key NOT expired.");

                        if (cacheEntry.Second >= RateLimitMax)
                        {
                            this.Log.Debug("Rate limit HIT");

                            // RATE LIMITED!
                            return true;
                        }

                        this.Log.Debug("Rate limit incremented.");

                        // increment counter
                        cacheEntry.Second++;
                    }
                    else
                    {
                        this.Log.Debug("Rate limit key is expired, resetting to new value.");

                        // Cache expired
                        cacheEntry.First = DateTime.Now;
                        cacheEntry.Second = 1;
                    }
                }
                else
                {
                    this.Log.Debug("Rate limit not found, creating key.");

                    // Not in cache.
                    var cacheEntry = new RateLimitCacheEntry { First = DateTime.Now, Second = 1 };
                    RateLimitCache.Add(this.Source.Hostname, cacheEntry);
                }

                // Clean up the cache
                foreach (var key in RateLimitCache.Keys.ToList())
                {
                    if (RateLimitCache[key].First.AddMinutes(RateLimitDuration) < DateTime.Now)
                    {
                        // Expired.
                        RateLimitCache.Remove(key);
                    }
                }
            }

            return false;
        }

        #endregion
    }
}