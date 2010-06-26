#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Uptime : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string[] messageParams = {
                                         Helpmebot6.StartupTime.DayOfWeek.ToString(),
                                         Helpmebot6.StartupTime.ToLongDateString(),
                                         Helpmebot6.StartupTime.ToLongTimeString()
                                     };
            string message = Configuration.singleton().getMessage("cmdUptimeUpSince", messageParams);
            return new CommandResponseHandler(message);
        }
    }
}