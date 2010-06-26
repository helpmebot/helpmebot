#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Rawctcp : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string cmd = GlobalFunctions.popFromFront(ref args);
            string dst = GlobalFunctions.popFromFront(ref args);

            Helpmebot6.irc.ircPrivmsg(dst, IAL.wrapCTCP(cmd, string.Join(" ", args)));

            return null;
        }
    }
}