#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Myaccess : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponseHandler crh = new CommandResponseHandler();
            string[] cmdArgs = {source.ToString(), source.accessLevel.ToString()};
            crh.respond(Configuration.singleton().getMessage("cmdAccess", cmdArgs));
            return crh;
        }
    }
}