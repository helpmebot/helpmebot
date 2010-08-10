#region Usings

using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    internal class Status : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string[] messageParams = {
                                         args[0],
                                         WatcherController.instance().isWatcherInChannel(channel, args[0])
                                             ? Configuration.singleton().getMessage("enabled")
                                             : Configuration.singleton().getMessage("disabled"),
                                         WatcherController.instance().getDelay(args[0]).ToString()
                                     };

            return new CommandResponseHandler(Configuration.singleton().getMessage("keywordStatus", messageParams));
        }
    }
}