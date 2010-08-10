#region Usings

using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    internal class Enable : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            if (WatcherController.instance().addWatcherToChannel(args[0], channel))
                return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
            return new CommandResponseHandler(Configuration.singleton().getMessage("no-change"));
        }
    }
}