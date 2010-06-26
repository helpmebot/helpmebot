#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Autolink : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            bool global = false;


            if (args.Length > 0)
            {
                if (args[0].ToLower() == "@global")
                {
                    global = true;
                    GlobalFunctions.popFromFront(ref args);
                }
            }

            bool oldValue = bool.Parse( !global ? Configuration.singleton().retrieveLocalStringOption("autoLink", channel) : Configuration.singleton().retrieveGlobalStringOption("autoLink") );

            if (args.Length > 0)
            {
                string newValue = "global";
                switch (args[0].ToLower())
                {
                    case "enable":
                        newValue = "true";
                        break;
                    case "disable":
                        newValue = "false";
                        break;
                    case "global":
                        newValue = "global";
                        break;
                }
                if (newValue == oldValue.ToString().ToLower())
                {
                    return new CommandResponseHandler(Configuration.singleton().getMessage("no-change"),
                                                      CommandResponseDestination.PrivateMessage);
                }
                if (newValue == "global")
                {
                    Configuration.singleton().deleteLocalOption("autoLink", channel);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("defaultConfig"),
                                                      CommandResponseDestination.PrivateMessage);
                }
                if (!global)
                    Configuration.singleton().setLocalOption("autoLink", channel, newValue);
                else
                    Configuration.singleton().setGlobalOption("autoLink", newValue);
                return new CommandResponseHandler(Configuration.singleton().getMessage("done"),
                                                  CommandResponseDestination.PrivateMessage);
            }
            string[] mP = {"autolink", 1.ToString(), args.Length.ToString()};
            return new CommandResponseHandler(Configuration.singleton().getMessage("notEnoughParameters", mP),
                                              CommandResponseDestination.PrivateMessage);
        }
    }
}