namespace Helpmebot.CoreServices.Services.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using NHibernate;

    public class DatabaseMessageRepository : IMessageRepository
    {
        private readonly ISession databaseSession;
        private readonly ILogger logger;
        private readonly Dictionary<DatabaseMessageKey, DatabaseMessage> cache =
            new Dictionary<DatabaseMessageKey, DatabaseMessage>();

        public DatabaseMessageRepository(ISession databaseSession, ILogger logger)
        {
            this.databaseSession = databaseSession;
            this.logger = logger;
        }

        public bool SupportsWrite => true;
        public bool SupportsContext => true;
        public string RepositoryType => "database";

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

            var messageObject = this.databaseSession.QueryOver<DatabaseMessage>()
                .Where(x => x.MessageKey == key)
                .And(x => contextType == null || x.ContextType == contextType)
                .And(x => context == null || x.Context == context)
                .SingleOrDefault();

            if (messageObject == null)
            {
                messageObject = new DatabaseMessage
                {
                    Context = context,
                    ContextType = contextType,
                    MessageKey = key
                };
            }
            
            messageObject.Value = JsonSerializer.Serialize(value);
            messageObject.Format = 1;
            messageObject.LastUpdated = DateTime.UtcNow;

            this.databaseSession.SaveOrUpdate(messageObject);
            this.databaseSession.Flush();
            
            lock (this.cache)
            {
                if (this.cache.ContainsKey(databaseMessageKey))
                {
                    this.cache.Remove(databaseMessageKey);
                }

                this.cache.Add(databaseMessageKey, messageObject);
            }
        }

        public List<List<string>> Get(string key, string contextType, string context)
        {
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
                messageObject = this.databaseSession.QueryOver<DatabaseMessage>()
                    .Where(x => x.MessageKey == key)
                    .And(x => x.ContextType == contextType)
                    .And(x => x.Context == context)
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
                return null;
            }

            try
            {
                var data = JsonSerializer.Deserialize<List<List<string>>>(messageObject.Value);
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
            
            var messageObject = this.databaseSession.QueryOver<DatabaseMessage>()
                .Where(x => x.MessageKey == key)
                .And(x => contextType == null || x.ContextType == contextType)
                .And(x => context == null || x.Context == context)
                .SingleOrDefault();
            
            if (messageObject == null)
            {
                // nothing to delete?
                return;
            }
            
            this.databaseSession.Delete(messageObject);
            this.databaseSession.Flush();
        }

        public IEnumerable<string> GetAllKeys()
        {
            var selectList = this.databaseSession.QueryOver<DatabaseMessage>()
                .List()
                .Select(x => x.MessageKey);

            return selectList;
        }
    }
}