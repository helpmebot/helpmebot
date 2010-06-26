#region Usings

using System.Reflection;
using helpmebot6.Threading;

#endregion

namespace helpmebot6.Commands
{
    internal class Threadstatus : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string[] statuses = ThreadList.instance().getAllThreadStatus();
            CommandResponseHandler crh = new CommandResponseHandler();
            foreach (string item in statuses)
            {
                crh.respond(item);
            }
            return crh;
        }
    }
}