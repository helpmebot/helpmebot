namespace Helpmebot.CoreServices.Services.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using NHibernate;

    public class DatabaseMessageRepository : IMessageRepository
    {
        private readonly ISession databaseSession;
        private readonly ILogger logger;

        public DatabaseMessageRepository(ISession databaseSession, ILogger logger)
        {
            this.databaseSession = databaseSession;
            this.logger = logger;
        }

        private readonly Dictionary<DatabaseMessageKey, DatabaseMessage> cache =
            new Dictionary<DatabaseMessageKey, DatabaseMessage>();

        public bool SupportsWrite => true;
        public bool SupportsContext => true;
        
        public void Set(string key, string contextType, string context, List<List<string>> value)
        {
            var databaseMessageKey = new DatabaseMessageKey(contextType, context, key);
            
            lock (this.cache)
            {
                if (this.cache.ContainsKey(databaseMessageKey))
                {
                    this.cache.Remove(databaseMessageKey);
                }
            }

            throw new NotImplementedException();
        }

        public List<List<string>> Get(string key, string contextType, string context)
        {
            this.logger.TraceFormat("Searching for {0} / ({1}) {2}.", key, contextType, context);

            var objectKey = new DatabaseMessageKey(contextType, context, key);
            DatabaseMessage messageObject = null;
            bool inCache = false;
            
            lock (this.cache)
            {
                if (this.cache.ContainsKey(objectKey))
                {
                    messageObject = this.cache[objectKey];
                    inCache = true;
                }
            }

            if (!inCache)
            {
                this.logger.TraceFormat("Database lookup for {0} / ({1}) {2}.", key, contextType, context);

                messageObject = this.databaseSession.QueryOver<DatabaseMessage>()
                    .Where(x => x.MessageKey == key)
                    .And(x => contextType == null || x.ContextType == contextType)
                    .And(x => context == null || x.Context == context)
                    .And(x => x.Format == 1)
                    .SingleOrDefault();

                lock (this.cache)
                {
                    if (!this.cache.ContainsKey(objectKey))
                    {
                        this.cache.Add(objectKey, messageObject);
                    }
                }
            }

            if (messageObject == null)
            {
                this.logger.TraceFormat("{0} not found", key);
                return null;
            }

            try
            {
                this.logger.TraceFormat("{0} deserializing", key);
                var data = JsonSerializer.Deserialize<List<List<string>>>(messageObject.Value);
                this.logger.TraceFormat("{0} done, {1} items", key, data?.Count);
                return data;
            }
            catch (JsonException ex)
            {
                this.logger.ErrorFormat(ex, "Invalid message for {0} / ({1}) {2}.", key, contextType, context);
                return null;
            }
        }

        public void Remove(string key, string contextType, string context)
        {
            var databaseMessageKey = new DatabaseMessageKey(contextType, context, key);
            
            lock (this.cache)
            {
                if (this.cache.ContainsKey(databaseMessageKey))
                {
                    this.cache.Remove(databaseMessageKey);
                }
            }

            throw new NotImplementedException();
        }
    }
}