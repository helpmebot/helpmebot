#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Blockuser : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string name = string.Join(" ", args);

            string url = Configuration.singleton().retrieveLocalStringOption("wikiUrl", channel);

            return new CommandResponseHandler(url + "Special:BlockIP/" + name);
        }
    }
}