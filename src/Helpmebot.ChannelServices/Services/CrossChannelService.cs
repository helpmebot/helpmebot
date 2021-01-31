namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Commands.CrossChannel;
    using Helpmebot.ChannelServices.Model;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class CrossChannelService : CommandParserProviderServiceBase<CrossChannel>, ICrossChannelService
    {
        private readonly ISession databaseSession;
        private readonly CrossChannelServiceConfiguration config;
        private readonly object sessionLock = new object();
        
        private static readonly Dictionary<string, RateLimitCacheEntry> RateLimitCache =
            new Dictionary<string, RateLimitCacheEntry>();

        public CrossChannelService(
            ISession databaseSession,
            ILogger logger,
            ICommandParser commandParser,
            CrossChannelServiceConfiguration config)
            : base(commandParser, logger)
        {
            this.databaseSession = databaseSession;
            this.config = config;
        }

        public void Configure(Channel frontend, Channel backend, ISession localSession)
        {
            var existing = localSession.CreateCriteria<CrossChannel>()
                .Add(
                    Restrictions.Or(
                        Restrictions.Or(
                            Restrictions.Eq("FrontendChannel", frontend),
                            Restrictions.Eq("FrontendChannel", backend)),
                        Restrictions.Or(
                            Restrictions.Eq("BackendChannel", frontend),
                            Restrictions.Eq("BackendChannel", backend))
                    ))
                .List<CrossChannel>();

            if (existing.Any())
            {
                throw new Exception(
                    "At least one of the channels requested is already involved in a cross-channel configuration.");
            }

            var cc = new CrossChannel
            {
                FrontendChannel = frontend,
                BackendChannel = backend,
                NotifyEnabled = false,
                ForwardEnabled = false
            };

            localSession.Save(cc);
        }

        public void Deconfigure(Channel backend, ISession localSession)
        {
            var existing = localSession.CreateCriteria<CrossChannel>()
                .Add(Restrictions.Eq("BackendChannel", backend))
                .UniqueResult<CrossChannel>();

            if (existing == null)
            {
                throw new Exception("Cannot find cross-channel configuration for this channel.");
            }

            if (existing.NotifyEnabled)
            {
                this.UnregisterCommand(existing);
            }

            localSession.Delete(existing);
        }

        public void NotificationStatus(Channel backend, bool status, ISession localSession)
        {
            var existing = localSession.CreateCriteria<CrossChannel>()
                .Add(Restrictions.Eq("BackendChannel", backend))
                .UniqueResult<CrossChannel>();

            if (existing == null)
            {
                throw new Exception("Cannot find cross-channel configuration for this channel.");
            }

            if (existing.NotifyEnabled == status)
            {
                // no-op
                return;
            }

            if (status && string.IsNullOrWhiteSpace(existing.NotifyKeyword))
            {
                throw new Exception("Cannot enable notifications before keyword is configured.");
            }

            if (status && string.IsNullOrWhiteSpace(existing.NotifyMessage))
            {
                throw new Exception("Cannot enable notifications before message is configured.");
            }

            existing.NotifyEnabled = status;
            localSession.Update(existing);

            if (status)
            {
                this.RegisterCommand(existing);
            }
            else
            {
                this.UnregisterCommand(existing);
            }
        }

        public void NotificationMessage(Channel backend, string message, ISession localSession)
        {
            var existing = localSession.CreateCriteria<CrossChannel>()
                .Add(Restrictions.Eq("BackendChannel", backend))
                .UniqueResult<CrossChannel>();

            if (existing == null)
            {
                throw new Exception("Cannot find cross-channel configuration for this channel.");
            }

            if (existing.NotifyMessage == message)
            {
                // no-op
                return;
            }

            existing.NotifyMessage = message;
            localSession.Update(existing);
        }

        public void NotificationKeyword(Channel backend, string keyword, ISession localSession)
        {
            var existing = localSession.CreateCriteria<CrossChannel>()
                .Add(Restrictions.Eq("BackendChannel", backend))
                .UniqueResult<CrossChannel>();

            if (existing == null)
            {
                throw new Exception("Cannot find cross-channel configuration for this channel.");
            }

            if (existing.NotifyKeyword == keyword)
            {
                // no-op
                return;
            }

            if (existing.NotifyEnabled)
            {
                throw new Exception("Cannot change notification keyword while notifications are enabled.");
            }

            existing.NotifyKeyword = keyword;
            localSession.Update(existing);
        }

        public void Notify(Channel frontend, string message, ISession localSession, IIrcClient client, IUser user)
        {
            var existing = localSession.CreateCriteria<CrossChannel>()
                .Add(Restrictions.Eq("FrontendChannel", frontend))
                .UniqueResult<CrossChannel>();

            if (existing == null)
            {
                this.Logger.ErrorFormat("Attempted notification for non-existent configuration.");
                throw new Exception("Cannot find cross-channel configuration for this channel.");
            }

            if (this.RateLimit(user, existing.Id))
            {
                this.Logger.InfoFormat("Rate limited notification response for {0} in {1}", user, frontend);
                return;
            }
            
            client.SendNotice(
                existing.BackendChannel.Name,
                string.Format(existing.NotifyMessage, user.Nickname, frontend.Name, message));
        }

        protected override IList<CrossChannel> ItemsToRegister()
        {
            IList<CrossChannel> itemsToRegister;
            lock (this.sessionLock)
            {
                itemsToRegister = this.databaseSession.CreateCriteria<CrossChannel>()
                    .Add(Restrictions.Eq("NotifyEnabled", true))
                    .List<CrossChannel>();
            }

            return itemsToRegister;
        }

        protected override Type CommandImplementation()
        {
            return typeof(CrossChannelNotifyCommand);
        }

        private bool RateLimit(IUser source, int id)
        {
            if (source == null || source.Hostname == null)
            {
                this.Logger.Error("Rate limiting called with no source or no source hostname!");
                return true;
            }

            var cacheKey = string.Format("{1}|{0}", source.Hostname, id);

            lock (RateLimitCache)
            {
                if (RateLimitCache.ContainsKey(cacheKey))
                {
                    this.Logger.Debug("Rate limit key found.");

                    var cacheEntry = RateLimitCache[cacheKey];

                    if (cacheEntry.Expiry.AddMinutes(this.config.RateLimitDuration) >= DateTime.Now)
                    {
                        this.Logger.Debug("Rate limit key NOT expired.");

                        if (cacheEntry.Counter >= this.config.RateLimitMax)
                        {
                            this.Logger.Debug("Rate limit HIT");

                            // RATE LIMITED!
                            return true;
                        }

                        this.Logger.Debug("Rate limit incremented.");

                        // increment counter
                        cacheEntry.Counter++;
                    }
                    else
                    {
                        this.Logger.Debug("Rate limit key is expired, resetting to new value.");

                        // Cache expired
                        cacheEntry.Expiry = DateTime.Now;
                        cacheEntry.Counter = 1;
                    }
                }
                else
                {
                    this.Logger.Debug("Rate limit not found, creating key.");

                    // Not in cache.
                    var cacheEntry = new RateLimitCacheEntry {Expiry = DateTime.Now, Counter = 1};
                    RateLimitCache.Add(cacheKey, cacheEntry);
                }

                // Clean up the cache
                foreach (var key in RateLimitCache.Keys.ToList())
                {
                    if (RateLimitCache[key].Expiry.AddMinutes(this.config.RateLimitDuration) < DateTime.Now)
                    {
                        // Expired.
                        RateLimitCache.Remove(key);
                    }
                }
            }

            return false;
        }
    }
}