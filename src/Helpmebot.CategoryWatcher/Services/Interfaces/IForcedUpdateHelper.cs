namespace Helpmebot.CategoryWatcher.Services.Interfaces
{
    using System.Collections.Generic;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;

    public interface IForcedUpdateHelper
    {
        IEnumerable<CommandResponse> DoForcedUpdate(
            string categoryKeyword,
            string channelName,
            bool suppressWarning);

        IEnumerable<CommandResponse> BulkForcedUpdate(bool all, string channelName);
    }
}