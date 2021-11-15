namespace Helpmebot.CoreServices.Services.Messages
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;

    public class MessageServiceShim : IMessageService
    {
        private readonly IResponder responder;
        private readonly ILogger logger;

        public MessageServiceShim(IResponder responder, ILogger logger)
        {
            this.responder = responder;
            this.logger = logger;
        }
        
        public string RetrieveMessage(string messageKey, object context, IEnumerable<string> arguments)
        {
            this.logger.WarnFormat("SHIM: {0} / {1}", messageKey, context);
            var commandResponses = this.responder
                .Respond(
                    messageKey,
                    context == null ? null : "channel",
                    (string)context,
                    arguments.ToArray())
                .ToList();

            if (!commandResponses.Any())
            {
                return null;
            }

            return commandResponses.First().CompileMessage();
        }
    }
}