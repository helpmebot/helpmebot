namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Commands.CrossChannel;
    using Helpmebot.ChannelServices.Configuration;
    using Helpmebot.ChannelServices.Model;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class CrossChannelService : CommandParserProviderServiceBase<CrossChannel>, ICrossChannelService
    {
        private readonly ISession databaseSession;
        private readonly RateLimitConfiguration config;
        private readonly object sessionLock = new object();
        
        private static readonly Dictionary<string, RateLimitCacheEntry> RateLimitCache =
            new Dictionary<string, RateLimitCacheEntry>();

        public CrossChannelService(
            ISession databaseSession,
            ILogger logger,
            ICommandParser commandParser,
            ModuleConfiguration config)
            : base(commandParser, logger)
        {
            this.databaseSession = databaseSession;
            this.config = config.CrossChannelRateLimits;
        }

        public void Configure(string frontend, string backend)
        {
            lock (this.sessionLock)
            {
                using (var txn = this.databaseSession.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var existing = this.databaseSession.CreateCriteria<CrossChannel>()
                        .Add(
                            Restrictions.Or(
                                Restrictions.Or(
                                    Restrictions.Eq(nameof(CrossChannel.FrontendChannel), frontend),
                                    Restrictions.Eq(nameof(CrossChannel.FrontendChannel), backend)),
                                Restrictions.Or(
                                    Restrictions.Eq(nameof(CrossChannel.BackendChannel), frontend),
                                    Restrictions.Eq(nameof(CrossChannel.BackendChannel), backend))
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

                    this.databaseSession.Save(cc);
                    txn.Commit();
                }
            }
        }

        public void Deconfigure(string backend)
        {
            lock (this.sessionLock)
            {
                using (var txn = this.databaseSession.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var existing = this.databaseSession.CreateCriteria<CrossChannel>()
                        .Add(Restrictions.Eq(nameof(CrossChannel.BackendChannel), backend))
                        .UniqueResult<CrossChannel>();

                    if (existing == null)
                    {
                        throw new Exception("Cannot find cross-channel configuration for this channel.");
                    }

                    if (existing.NotifyEnabled)
                    {
                        this.UnregisterCommand(existing);
                    }

                    this.databaseSession.Delete(existing);
                    txn.Commit();
                }
            }
        }

        public void SetNotificationStatus(string backend, bool status)
        {
            lock (this.sessionLock)
            {
                using (var txn = this.databaseSession.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var existing = this.databaseSession.CreateCriteria<CrossChannel>()
                        .Add(Restrictions.Eq(nameof(CrossChannel.BackendChannel), backend))
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
                    this.databaseSession.Update(existing);
                    txn.Commit();

                    if (status)
                    {
                        this.RegisterCommand(existing);
                    }
                    else
                    {
                        this.UnregisterCommand(existing);
                    }
                }
            }
        }

        public bool GetNotificationStatus(string backend)
        {
            lock (this.sessionLock)
            {
                var existing = this.databaseSession.CreateCriteria<CrossChannel>()
                    .Add(Restrictions.Eq(nameof(CrossChannel.BackendChannel), backend))
                    .UniqueResult<CrossChannel>();

                if (existing == null)
                {
                    throw new Exception("Cannot find cross-channel configuration for this channel.");
                }

                return existing.NotifyEnabled;
            }
        }

        public void SetNotificationMessage(string backend, string message)
        {
            lock (this.sessionLock)
            {
                using (var txn = this.databaseSession.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var existing = this.databaseSession.CreateCriteria<CrossChannel>()
                        .Add(Restrictions.Eq(nameof(CrossChannel.BackendChannel), backend))
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
                    this.databaseSession.Update(existing);
                    txn.Commit();
                }
            }
        }
        
        public void SetNotificationKeyword(string backend, string keyword)
        {
            lock (this.sessionLock)
            {
                using (var txn = this.databaseSession.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var existing = this.databaseSession.CreateCriteria<CrossChannel>()
                        .Add(Restrictions.Eq(nameof(CrossChannel.BackendChannel), backend))
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
                    this.databaseSession.Update(existing);
                    txn.Commit();
                }
            }
        }

        public void Notify(string frontend, string message, IIrcClient client, IUser user)
        {
            lock (this.sessionLock)
            {
                var existing = this.databaseSession.CreateCriteria<CrossChannel>()
                    .Add(Restrictions.Eq(nameof(CrossChannel.FrontendChannel), frontend))
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
                    existing.BackendChannel,
                    string.Format(existing.NotifyMessage, user.Nickname, frontend, message));
            }
        }

        public string GetBackendChannelName(string frontend)
        {
            lock (this.sessionLock)
            {
                var crossChannel = this.databaseSession.QueryOver<CrossChannel>()
                    .Where(x => x.FrontendChannel == frontend)
                    .SingleOrDefault();

                if (crossChannel != null)
                {
                    return crossChannel.BackendChannel;
                }
            }

            return null;
        }

        public string GetFrontendChannelName(string backend)
        {
            lock (this.sessionLock)
            {
                var crossChannel = this.databaseSession.QueryOver<CrossChannel>()
                    .Where(x => x.BackendChannel == backend)
                    .SingleOrDefault();

                if (crossChannel != null)
                {
                    return crossChannel.FrontendChannel;
                }
            }

            return null;
        }
        
        protected override IList<CrossChannel> ItemsToRegister()
        {
            IList<CrossChannel> itemsToRegister;
            lock (this.sessionLock)
            {
                itemsToRegister = this.databaseSession.CreateCriteria<CrossChannel>()
                    .Add(Restrictions.Eq(nameof(CrossChannel.NotifyEnabled), true))
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