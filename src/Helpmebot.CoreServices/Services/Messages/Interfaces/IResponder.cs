namespace Helpmebot.CoreServices.Services.Messages.Interfaces
{
    using System.Collections.Generic;
    using Castle.MicroKernel.Context;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;

    public interface IResponder
    {
        IEnumerable<CommandResponse> Respond(
            string messageKey,
            string context,
            params string[] arguments);
        
        IEnumerable<CommandResponse> Respond(
            string messageKey,
            string context,
            string[] arguments = null,
            Context contextType = null,
            CommandResponseDestination destination = CommandResponseDestination.Default,
            CommandResponseType type = CommandResponseType.Message,
            bool ignoreRedirection = false,
            IEnumerable<string> redirectionTarget = null
            );
    }
}