#region Usings

using System;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the current date/time
    /// </summary>
    internal class Time : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string[] messageParams = {
                                         source.nickname,
                                         DateTime.Now.DayOfWeek.ToString(),
                                         DateTime.Now.Year.ToString(),
                                         DateTime.Now.Month.ToString("00"),
                                         DateTime.Now.Day.ToString("00"),
                                         DateTime.Now.Hour.ToString("00"),
                                         DateTime.Now.Minute.ToString("00"),
                                         DateTime.Now.Second.ToString("00")
                                     };
            string message = Configuration.singleton().getMessage("cmdTime", messageParams);
            return new CommandResponseHandler(message);
        }
    }

    /// <summary>
    ///   Returns the current date/time. Alias for Time.
    /// </summary>
    internal class Date : Time
    {
    }
}