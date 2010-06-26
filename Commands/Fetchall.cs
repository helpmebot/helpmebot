#region Usings

using System.Collections.Generic;
using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands
{
    internal class Fetchall : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponseHandler crh = new CommandResponseHandler();
            Dictionary<string, Monitoring.CategoryWatcher>.KeyCollection kc = WatcherController.instance().getKeywords();
            if (GlobalFunctions.isInArray("@cats", args) != -1)
            {
                GlobalFunctions.removeItemFromArray("@cats", ref args);
                string listSep = Configuration.singleton().getMessage("listSeparator");
                string list = Configuration.singleton().getMessage("allCategoryCodes");
                foreach (string item in kc)
                {
                    list += item;
                    list += listSep;
                }

                crh.respond(list.TrimEnd(listSep.ToCharArray()));
            }
            else
            {
                foreach (string key in kc)
                {
                    crh.respond(WatcherController.instance().forceUpdate(key, channel));
                }
            }
            return crh;
        }
    }
}