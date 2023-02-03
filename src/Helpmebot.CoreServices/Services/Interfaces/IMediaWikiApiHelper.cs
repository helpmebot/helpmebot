namespace Helpmebot.CoreServices.Services.Interfaces
{
    using Helpmebot.Model;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public interface IMediaWikiApiHelper
    {
        IMediaWikiApi GetApi(MediaWikiSite site);
        void Release(IMediaWikiApi api);
    }
}