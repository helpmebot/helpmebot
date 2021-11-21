namespace Helpmebot.WebUI.Services
{
    using Helpmebot.WebUI.Models;

    public interface IStaticPageService
    {
        StaticPage Load(string pageName);
    }
}