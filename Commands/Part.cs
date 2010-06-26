#region Usings

using System.Collections.Generic;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Part : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            Helpmebot6.irc.ircPart(channel, source.ToString());
            Dictionary<string, string> vals = new Dictionary<string, string>
                                                  { { "channel_enabled", "0" } };
            DAL.singleton().update("channel", vals, 0, new DAL.WhereConds("channel_name", channel),
                                   new DAL.WhereConds("channel_network", source.network.ToString()));
            return null;
        }
    }
}