namespace Helpmebot.CoreServices.Services.Messages.Interfaces
{
    using System.Collections.Generic;

    public interface IMessageRepository
    {
        bool SupportsWrite { get; }
        bool SupportsContext { get; }

        void Set(string key, string contextType, string context, List<List<string>> value);
        List<List<string>> Get(string key, string contextType, string context);
        void Remove(string key, string contextType, string context);
    }
}