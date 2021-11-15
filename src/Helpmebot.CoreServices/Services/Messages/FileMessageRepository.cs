namespace Helpmebot.CoreServices.Services.Messages
{
    using System;
    using System.Collections.Generic;

    public class FileMessageRepository : ReadOnlyMessageRepository
    {
        public override bool SupportsContext => false;
        
        public override List<List<string>> Get(string key, string contextType, string context)
        {
            throw new NotImplementedException();
        }
    }
}