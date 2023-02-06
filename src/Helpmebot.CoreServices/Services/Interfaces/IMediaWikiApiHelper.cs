namespace Helpmebot.CoreServices.Services.Interfaces
{
    using System;
    using Helpmebot.Model;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public interface IMediaWikiApiHelper
    {
        [Obsolete("Legacy MW site database config; use CMS.GetBaseWiki() and MWAPIHelper.")]
        IMediaWikiApi GetApi(MediaWikiSite site);
        void Release(IMediaWikiApi api);
        IMediaWikiApi GetApi(string siteId, bool fallback = false);
    }
}