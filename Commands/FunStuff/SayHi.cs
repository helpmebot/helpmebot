#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Says hi to a user
    /// </summary>
    internal class Sayhi : GenericCommand
    {
        protected override CommandResponseHandler execute(User toUser, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string[] commandParams = {toUser.nickname};
            return new CommandResponseHandler(Configuration.singleton().getMessage("cmdSayHi1", commandParams));
        }
    }
}