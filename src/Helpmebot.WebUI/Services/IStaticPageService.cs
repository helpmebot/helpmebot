namespace Helpmebot.WebUI.Services
{
    using System.Collections.Generic;
    using Helpmebot.WebUI.Models;

    public interface IStaticPageService
    {
        bool Exists(string route);
        StaticPage GetPage(string route);
        List<StaticPage> GetNavEntries();
    }
}