namespace Helpmebot.Services.Interfaces
{
    using System.Collections.Generic;

    public interface IMediaWikiBotService
    {
        void Login();
        string GetPage(string pageName, out string timestamp);
        bool WritePage(string pageName, string content, string editSummary, string timestamp, bool bot, bool minor);
        void DeletePage(string pageName, string reason);
        IEnumerable<string> PrefixSearch(string prefix);
    }
}