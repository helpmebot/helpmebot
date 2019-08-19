namespace Helpmebot.Services.Interfaces
{
    using System;
    using Helpmebot.Model;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public interface IDraftStatusService
    {
        DraftStatus GetDraftStatus(IMediaWikiApi mediaWikiApi, string page);
        DateTime? GetOldestDraft(IMediaWikiApi mediaWikiApi);
        int GetPendingDraftCount(IMediaWikiApi mediaWikiApi);
    }
}