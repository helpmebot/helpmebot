#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Trout : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string name = string.Join(" ", args);

            string[] forbiddenTargets = {
                                            "stwalkerster", "itself", "himself", "herself",
                                            Helpmebot6.irc.ircNickname.ToLower()
                                        };

            if (GlobalFunctions.isInArray(name.ToLower(), forbiddenTargets) != -1)
            {
                name = source.nickname;
            }

            string[] messageparams = {name};
            string message = IAL.wrapCTCP("ACTION", Configuration.singleton().getMessage("cmdTrout", messageparams));

            return new CommandResponseHandler(message);
        }
    }
}