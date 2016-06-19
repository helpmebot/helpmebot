// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JoinMessageService.cs" company="Helpmebot Development Team">
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
//   Defines the JoinMessageService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Castle.Core.Logging;

    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    using Cache = System.Collections.Generic.Dictionary<string, Model.RateLimitCacheEntry>;

    /// <summary>
    /// The join message service.
    /// </summary>
    public class JoinMessageService : IJoinMessageService
    {
        /// <summary>
        /// The rate limit max.
        /// </summary>
        /// <remarks>
        /// TODO: push this into a config option
        /// </remarks>
        private const int RateLimitMax = 1;

        /// <summary>
        /// The rate limit duration in minutes
        /// </summary>
        /// <remarks>
        /// TODO: push this into a config option
        /// </remarks>
        private const int RateLimitDuration = 10;

        /// <summary>
        /// The rate limit cache.
        /// </summary>
        private readonly Dictionary<string, Cache> rateLimitCache = new Dictionary<string, Cache>();

        /// <summary>
        /// The IRC network.
        /// </summary>
        private readonly IIrcClient ircClient;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// The session.
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initialises a new instance of the <see cref="JoinMessageService"/> class.
        /// </summary>
        /// <param name="ircClient">
        /// The IRC network.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public JoinMessageService(IIrcClient ircClient, ILogger logger, IMessageService messageService, ISession session)
        {
            this.ircClient = ircClient;
            this.logger = logger;
            this.messageService = messageService;
            this.session = session;
        }

        /// <summary>
        /// The welcome.
        /// </summary>
        /// <param name="networkUser">
        /// The network User.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        public void Welcome(IUser networkUser, string channel)
        {
            // Rate limit this per hostname/channel
            if (this.RateLimit(networkUser.Username, channel))
            {
                return;
            }

            // status
            bool match = false;

            this.logger.DebugFormat("Searching for welcome matches for {0} in {1}...", networkUser, channel);

            var users = this.GetWelcomeUsers(channel);

            if (users.Any())
            {
                foreach (var welcomeUser in users)
                {
                    Match nick = new Regex(welcomeUser.Nick).Match(networkUser.Nickname);
                    Match user = new Regex(welcomeUser.User).Match(networkUser.Username);
                    Match host = new Regex(welcomeUser.Host).Match(networkUser.Hostname);

                    if (nick.Success && user.Success && host.Success)
                    {
                        this.logger.DebugFormat(
                            "Found a match for {0} in {1} with {2}",
                            networkUser,
                            channel,
                            welcomeUser);
                        match = true;
                        break;
                    }
                }
            }

            if (!match)
            {
                this.logger.InfoFormat("No welcome matches found for {0} in {1}.", networkUser, channel);
                return;
            }

            this.logger.DebugFormat("Searching for exception matches for {0} in {1}...", networkUser, channel);

            var exceptions = this.GetExceptions(channel);

            if (exceptions.Any())
            {
                foreach (var welcomeUser in exceptions)
                {
                    Match nick = new Regex(welcomeUser.Nick).Match(networkUser.Nickname);
                    Match user = new Regex(welcomeUser.User).Match(networkUser.Username);
                    Match host = new Regex(welcomeUser.Host).Match(networkUser.Hostname);

                    if (nick.Success && user.Success && host.Success)
                    {
                        this.logger.DebugFormat(
                            "Found an exception match for {0} in {1} with {2}",
                            networkUser,
                            channel,
                            welcomeUser);

                        return;
                    }
                }
            }

            this.logger.InfoFormat("Welcoming {0} into {1}...", networkUser, channel);

            var welcomeMessage = this.messageService.RetrieveMessage(
                "WelcomeMessage",
                channel,
                new[] { networkUser.Nickname, channel });

            this.ircClient.SendMessage(channel, welcomeMessage);
        }

        /// <summary>
        /// The get exceptions.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public virtual IList<WelcomeUser> GetExceptions(string channel)
        {
            var exceptions = this.session.QueryOver<WelcomeUser>().Where(x => x.Channel == channel && x.Exception).List();
            return exceptions;
        }

        /// <summary>
        /// The get welcome users.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public virtual IList<WelcomeUser> GetWelcomeUsers(string channel)
        {
            var users = this.session.QueryOver<WelcomeUser>().Where(x => x.Channel == channel && x.Exception == false).List();
            return users;
        }

        /// <summary>
        /// The clear rate limit cache.
        /// </summary>
        public void ClearRateLimitCache()
        {
            lock (this.rateLimitCache)
            {
                this.rateLimitCache.Clear();
            }
        }

        /// <summary>
        /// The rate limit.
        /// </summary>
        /// <param name="hostname">
        /// The hostname.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// true if rate limited, false otherwise
        /// </returns>
        private bool RateLimit(string hostname, string channel)
        {
            if (string.IsNullOrEmpty(hostname) || string.IsNullOrEmpty(channel))
            {
                // sanity check - this probably chanserv.
                this.logger.Error("JoinMessage ratelimiting called with null channel or null hostname!");
                return true;
            }

            try
            {
                // TODO: rate limiting needs to be tidyed up a bit
                lock (this.rateLimitCache)
                {
                    if (!this.rateLimitCache.ContainsKey(channel))
                    {
                        this.rateLimitCache.Add(channel, new Cache());
                    }

                    var channelCache = this.rateLimitCache[channel];

                    if (channelCache.ContainsKey(hostname))
                    {
                        this.logger.Debug("Rate limit key found.");

                        var cacheEntry = channelCache[hostname];

                        if (cacheEntry.Expiry.AddMinutes(RateLimitDuration) >= DateTime.Now)
                        {
                            this.logger.Debug("Rate limit key NOT expired.");

                            if (cacheEntry.Counter >= RateLimitMax)
                            {
                                this.logger.Debug("Rate limit HIT");

                                // RATE LIMITED!
                                return true;
                            }

                            this.logger.Debug("Rate limit incremented.");

                            // increment counter
                            cacheEntry.Counter++;
                        }
                        else
                        {
                            this.logger.Debug("Rate limit key is expired, resetting to new value.");

                            // Cache expired
                            cacheEntry.Expiry = DateTime.Now;
                            cacheEntry.Counter = 1;
                        }
                    }
                    else
                    {
                        this.logger.Debug("Rate limit not found, creating key.");

                        // Not in cache.
                        var cacheEntry = new RateLimitCacheEntry { Expiry = DateTime.Now, Counter = 1 };
                        channelCache.Add(hostname, cacheEntry);
                    }

                    // Clean up the channel's cache.
                    foreach (var key in channelCache.Keys.ToList())
                    {
                        if (channelCache[key].Expiry.AddMinutes(RateLimitDuration) < DateTime.Now)
                        {
                            // Expired.
                            channelCache.Remove(key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ooh dear. Something went wrong with rate limiting.
                this.logger.Error("Unknown error during rate limit processing.", ex);
                return false;
            }

            return false;
        }
    }
}
