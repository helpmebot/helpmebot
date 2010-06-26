#region Usings

using System.Collections;
using System.Reflection;

#endregion

namespace helpmebot6
{
    internal enum CommandResponseDestination
    {
        Default,
        ChannelDebug,
        PrivateMessage
    }

    internal struct CommandResponse
    {
        public CommandResponseDestination destination;
        public string message;
    }

    internal class CommandResponseHandler
    {
        private readonly ArrayList _responses;

        public CommandResponseHandler()
        {
            this._responses = new ArrayList();
        }

        public CommandResponseHandler(string message)
        {
            this._responses = new ArrayList();
            respond(message);
        }

        public CommandResponseHandler(string message, CommandResponseDestination destination)
        {
            this._responses = new ArrayList();
            respond(message, destination);
        }

        public void respond(string message)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponse cr;
            cr.destination = CommandResponseDestination.Default;
            cr.message = message;

            this._responses.Add(cr);
        }

        public void respond(string message, CommandResponseDestination destination)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponse cr;
            cr.destination = destination;
            cr.message = message;

            this._responses.Add(cr);
        }

        public void append(CommandResponseHandler moreResponses)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            foreach (object item in moreResponses.getResponses())
            {
                this._responses.Add(item);
            }
        }

        public ArrayList getResponses()
        {
            return this._responses;
        }
    }
}