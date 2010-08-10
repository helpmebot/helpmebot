#region Usings

using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    internal class Delay : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            if (args.Length > 2)
            {
                // 2 or more args
                return WatcherController.instance().setDelay(args[0], int.Parse(args[2]));
            }
            if (args.Length == 2)
            {
                int delay = WatcherController.instance().getDelay(args[0]);
                string[] messageParams = {args[0], delay.ToString()};
                string message = Configuration.singleton().getMessage("catWatcherCurrentDelay", messageParams);
                return new CommandResponseHandler(message);
            }
            // TODO: fix
            return null;
        }
    }
}