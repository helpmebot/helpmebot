namespace Helpmebot.CoreServices.Services.Messages
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;

    public class WikiMessageRepository : ReadOnlyMessageRepository
    {
        private class WikiMessage
        {
            public DatabaseMessageKey Key { get; set; }
            public List<List<string>> Values { get; set; }
        }

        private readonly ILegacyMessageBackend legacyMessageBackend;
        private readonly ILogger logger;
        private readonly Dictionary<DatabaseMessageKey, WikiMessage> cache =
            new Dictionary<DatabaseMessageKey, WikiMessage>();

        public WikiMessageRepository(ILegacyMessageBackend legacyMessageBackend, ILogger logger)
        {
            this.legacyMessageBackend = legacyMessageBackend;
            this.logger = logger;
        }

        public override bool SupportsContext => true;
        public override string RepositoryType => "wiki";

        public override List<List<string>> Get(string key, string contextType, string context)
        {
            var objectKey = new DatabaseMessageKey(contextType, context, key);
            WikiMessage messageObject = null;
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
                var contextData = string.Empty;
                if (!string.IsNullOrEmpty(context))
                {
                    contextData = $"/{context}";

                    contextData = contextData.Replace("#", string.Empty) // will cause issues
                        .Replace("|", string.Empty) // link syntax
                        .Replace("[", string.Empty) // link syntax
                        .Replace("]", string.Empty) // link syntax
                        .Replace("{", string.Empty) // link syntax
                        .Replace("}", string.Empty) // link syntax
                        .Replace("<", string.Empty) // html issues
                        .Replace(">", string.Empty); // html issues
                }

                // normalise message name to account for old messages
                if (key.Substring(0, 1).ToUpper() != key.Substring(0, 1))
                {
                    key = key.Substring(0, 1).ToUpper() + key.Substring(1);
                }

                var results = this.legacyMessageBackend.GetRawMessages(string.Concat(key, contextData)).ToList();

                if (results.Any())
                {
                    this.logger.ErrorFormat("Using wiki message as fallback for key {0}", key);
                    messageObject = new WikiMessage
                        { Key = objectKey, Values = results.Select(x => new List<string> { x }).ToList() };
                }

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

            return messageObject.Values;
        }

        public override IEnumerable<string> GetAllKeys()
        {
            return this.legacyMessageBackend.GetAllKeys();
        }
    }
}