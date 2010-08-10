#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Uptime : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
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