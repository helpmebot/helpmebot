#region Usings

using System.Reflection;
using helpmebot6.Monitoring;

#endregion

namespace helpmebot6.Commands
{
    internal class Welcomer : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            switch (args[0].ToLower())
            {
                case "enable":
                    if (Configuration.singleton().retrieveLocalStringOption("welcomeNewbie", channel) == "true")
                    {
                        return new CommandResponseHandler(Configuration.singleton().getMessage("no-change"));
                    }
                    Configuration.singleton().setLocalOption("welcomeNewbie", channel, "true");
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "disable":
                    if (Configuration.singleton().retrieveLocalStringOption("welcomeNewbie", channel) == "false")
                    {
                        return new CommandResponseHandler(Configuration.singleton().getMessage("no-change"));
                    }
                    Configuration.singleton().setLocalOption("welcomeNewbie", channel, "false");
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "global":
                    Configuration.singleton().deleteLocalOption("welcomeNewbie", channel);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("defaultSetting"));
                case "addOrder":
                    NewbieWelcomer.instance().addHost(args[1]);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "del":
                    NewbieWelcomer.instance().delHost(args[1]);
                    return new CommandResponseHandler(Configuration.singleton().getMessage("done"));
                case "list":
                    CommandResponseHandler crh = new CommandResponseHandler();
                    string[] list = NewbieWelcomer.instance().getHosts();
                    foreach (string item in list)
                    {
                        crh.respond(item);
                    }
                    return crh;
            }
            return new CommandResponseHandler();
        }
    }
}