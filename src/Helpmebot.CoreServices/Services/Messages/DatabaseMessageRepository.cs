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
        
        public bool SupportsWrite => true;
        public bool SupportsContext => true;
        
        public void Set(string key, string contextType, string context, List<List<string>> value)
        {
            throw new NotImplementedException();
        }

        public List<List<string>> Get(string key, string contextType, string context)
        {
            this.logger.TraceFormat("Searching for {0} / ({1}) {2}.", key, contextType, context);
            
            var message = this.databaseSession.QueryOver<DatabaseMessage>()
                .Where(x => x.MessageKey == key)
                .And(x => contextType == null || x.ContextType == contextType)
                .And(x => context == null || x.Context == context)
                .SingleOrDefault();

            if (message == null)
            {
                this.logger.TraceFormat("{0} not found", key);
                return null;
            }

            try
            {
                this.logger.TraceFormat("{0} deserializing", key);
                var data = JsonSerializer.Deserialize<List<List<string>>>(message.Value);
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
            throw new NotImplementedException();
        }
    }
}