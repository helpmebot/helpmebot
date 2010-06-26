#region Usings

using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    internal class Disable : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            WatcherController.instance().removeWatcherFromChannel(args[0], channel);
            return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
        }
    }
}