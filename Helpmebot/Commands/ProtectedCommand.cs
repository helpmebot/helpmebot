using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6.Commands
{
    abstract class ProtectedCommand : GenericCommand
    {
        protected override CommandResponseHandler reallyRun(User source, string channel, string[] args)
        {

            if(!AccessLog.instance().save(new AccessLog.AccessLogEntry(source, GetType(), true,channel, args)))
            {
                CommandResponseHandler errorResponse = new CommandResponseHandler();
                errorResponse.respond("Error adding to access log - command aborted.", CommandResponseDestination.ChannelDebug);
                errorResponse.respond(new Message().get("AccessDeniedAccessListFailure"), CommandResponseDestination.Default);
                return errorResponse;
            }

            this.log("Starting command execution...");
            CommandResponseHandler crh;

            try
            {
                crh = GlobalFunctions.isInArray("@confirm", args) != -1 ? execute(source, channel, args) : notConfirmed(source, channel, args);
            }
            catch (Exception ex)
            {
                Logger.instance().addToLog(ex.ToString(), Logger.LogTypes.Error);
                crh = new CommandResponseHandler(ex.Message);
            }
            this.log("Command execution complete.");
            return crh;
        }

        protected abstract CommandResponseHandler notConfirmed(User source, string channel, string[] args);
    }
}
