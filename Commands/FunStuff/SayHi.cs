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
            string[] commandParams = {toUser.nickname};
            return new CommandResponseHandler(Configuration.singleton().getMessage("cmdSayHi1", commandParams));
        }
    }
}