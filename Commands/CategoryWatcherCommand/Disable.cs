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
            WatcherController.instance().removeWatcherFromChannel(args[0], channel);
            return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
        }
    }
}