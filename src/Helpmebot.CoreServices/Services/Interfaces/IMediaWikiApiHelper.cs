namespace Helpmebot.CoreServices.Services.Interfaces
{
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public interface IMediaWikiApiHelper
    {
        void Release(IMediaWikiApi api);
        IMediaWikiApi GetApi(string siteId, bool fallback = true);
    }
}