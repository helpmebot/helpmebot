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