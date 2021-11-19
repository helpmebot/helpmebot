namespace Helpmebot.CoreServices.Services.Messages
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;

    public class WikiMessageRepository : ReadOnlyMessageRepository
    {
        private readonly ILegacyMessageBackend legacyMessageBackend;
        private readonly ILogger logger;

        public WikiMessageRepository(ILegacyMessageBackend legacyMessageBackend, ILogger logger)
        {
            this.legacyMessageBackend = legacyMessageBackend;
            this.logger = logger;
        }

        public override bool SupportsContext => true;
        public override string RepositoryType => "wiki";

        public override List<List<string>> Get(string key, string contextType, string context)
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
            if (!results.Any())
            {
                return null;
            }

            this.logger.ErrorFormat("Using wiki message as fallback for key {0}", key);

            return results.Select(x => new List<string> { x }).ToList();
        }

        public override IEnumerable<string> GetAllKeys()
        {
            return this.legacyMessageBackend.GetAllKeys();
        }
    }
}