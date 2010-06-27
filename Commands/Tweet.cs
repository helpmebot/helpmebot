#region Usings
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Tweet : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            Twitter twit = Helpmebot6.twitter;


            string status = string.Join(" ", args);

            twit.updateStatus(status);
            return new CommandResponseHandler( Configuration.singleton( ).getMessage( "done" ) );
        }
    }
}