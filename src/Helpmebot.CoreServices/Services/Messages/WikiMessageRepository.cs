namespace Helpmebot.CoreServices.Services.Messages
{
    using System.Collections.Generic;
    using System.Linq;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;

    public class WikiMessageRepository : ReadOnlyMessageRepository
    {
        private readonly ILegacyMessageBackend legacyMessageBackend;

        public WikiMessageRepository(ILegacyMessageBackend legacyMessageBackend)
        {
            this.legacyMessageBackend = legacyMessageBackend;
        }

        public override bool SupportsContext => true;

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

            return results.Select(x => new List<string> { x }).ToList();
        }
    }
}