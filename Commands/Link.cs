#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Link : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                string.Format("Method:{0}{1}", MethodBase.GetCurrentMethod().DeclaringType.Name,
                              MethodBase.GetCurrentMethod().Name), Logger.LogTypes.DNWB);

            bool secure = bool.Parse(Configuration.singleton().retrieveLocalStringOption("useSecureWikiServer", channel));
            if (args.Length > 0)
            {
                if (args[0] == "@secure")
                {
                    secure = true;
                    GlobalFunctions.popFromFront(ref args);
                }
            }

            string key = channel;
            if (GlobalFunctions.realArrayLength(args) > 0)
            {
                key = "<<<REALTIME>>>";
                Linker.instance().parseMessage(string.Join(" ", args), key);
            }

            return new CommandResponseHandler(Linker.instance().getLink(key, secure));
        }
    }
}