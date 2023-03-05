namespace Helpmebot.CoreServices.Services.Messages.Interfaces
{
    using System.Collections.Generic;

    public interface IResponseManager
    {
        void Set(string messageKey, string contextType, string context, List<List<string>> messageData);
        
        (List<List<string>>, string) Get(string messageKey, string contextType, string context);

        void Remove(string messageKey, string contextType, string context);

        List<string> GetAllKeys();

        void RegisterRepository(IMessageRepository repository);
    }
}