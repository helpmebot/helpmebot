namespace Helpmebot.CoreServices.Services.Messages.Interfaces
{
    using System.Collections.Generic;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;

    public interface IResponder
    {
        IEnumerable<CommandResponse> Respond(
            string messageKey,
            string contextType,
            string context,
            params string[] arguments);
    }
}