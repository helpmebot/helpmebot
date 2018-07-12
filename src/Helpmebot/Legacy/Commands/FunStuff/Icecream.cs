namespace helpmebot6.Commands
{
    using Helpmebot.Commands.FunStuff;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;

    public class Icecream : TargetedFunCommand
    {
        public Icecream(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper) : base(source, channel, args, commandServiceHelper)
        {
        }

        protected override string TargetMessage
        {
            get { return "CmdIcecream"; }
        }
    }
}