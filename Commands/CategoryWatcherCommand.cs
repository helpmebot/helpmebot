#region Usings

using System;
using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands
{
    internal class CategoryWatcher : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            CommandResponseHandler crh = new CommandResponseHandler();

            if (args.Length == 1)
            {
                // just do category check
                crh.respond(WatcherController.instance().forceUpdate(args[0], channel));
            }
            else
            {
                // do something else too.
                Type subCmdType =
                    Type.GetType("helpmebot6.Commands.CategoryWatcherCommand." + args[1].Substring(0, 1).ToUpper() +
                                 args[1].Substring(1).ToLower());
                if (subCmdType != null)
                {
                    return ((GenericCommand) Activator.CreateInstance(subCmdType)).run(source, channel, args);
                }
            }
            return crh;
        }
    }
}