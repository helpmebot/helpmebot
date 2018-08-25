namespace helpmebot6.Commands
{
    using Helpmebot.Commands.FunStuff;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [LegacyCommandFlag(LegacyUserRights.Advanced)]
    public class Icecream : TargetedFunCommand
    {
        public Icecream(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper) : base(source, channel, args, commandServiceHelper)
        {
        }

        protected override string TargetMessage
        {
            get { return "CmdIcecream"; }
        }
    }
}