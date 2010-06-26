#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Forgets a keyword
    /// </summary>
    internal class Forget : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (args.Length >= 1)
            {
                if (WordLearner.forget(args[0]))
                    Helpmebot6.irc.ircNotice(source.nickname, Configuration.singleton().getMessage("cmdForgetDone"));
                else
                    Helpmebot6.irc.ircNotice(source.nickname, Configuration.singleton().getMessage("cmdForgetError"));
            }
            else
            {
                string[] messageParameters = {"forget", "1", args.Length.ToString()};
                Helpmebot6.irc.ircNotice(source.nickname,
                                         Configuration.singleton().getMessage("notEnoughParameters", messageParameters));
            }
            return null;
        }
    }
}