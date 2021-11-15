namespace Helpmebot.CoreServices.Services.Messages
{
    using System.Collections.Generic;
    using System.Data;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;

    public abstract class ReadOnlyMessageRepository : IMessageRepository
    {
        public bool SupportsWrite => false;
        public abstract bool SupportsContext { get; }
        
        public abstract List<List<string>> Get(string key, string contextType, string context);

        public void Set(string key, string contextType, string context, List<List<string>> value)
        {
            throw new ReadOnlyException();
        }

        public void Remove(string key, string contextType, string context)
        {
            throw new ReadOnlyException();
        }
    }
}