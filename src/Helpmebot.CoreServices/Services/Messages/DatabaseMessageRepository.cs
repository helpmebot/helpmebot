namespace Helpmebot.CoreServices.Services.Messages
{
    using System;
    using System.Collections.Generic;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;

    public class DatabaseMessageRepository : IMessageRepository
    {
        public bool SupportsWrite => true;
        public bool SupportsContext => true;
        
        public void Set(string key, string contextType, string context, List<List<string>> value)
        {
            throw new NotImplementedException();
        }

        public List<List<string>> Get(string key, string contextType, string context)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key, string contextType, string context)
        {
            throw new NotImplementedException();
        }
    }
}