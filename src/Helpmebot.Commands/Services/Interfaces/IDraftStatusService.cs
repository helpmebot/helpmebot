namespace Helpmebot.Commands.Services.Interfaces
{
    using System;
    using Helpmebot.Commands.Model;
    using Helpmebot.Model;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public interface IDraftStatusService
    {
        DraftStatus GetDraftStatus(IMediaWikiApi mediaWikiApi, string page);
        (DateTime? date, int categorySize) GetOldestDraft(IMediaWikiApi mediaWikiApi);
        int GetPendingDraftCount(IMediaWikiApi mediaWikiApi);
    }
}