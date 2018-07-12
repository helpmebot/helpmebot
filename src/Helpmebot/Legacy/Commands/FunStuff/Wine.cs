namespace helpmebot6.Commands
{
    using Helpmebot.Commands.FunStuff;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;

    public class Wine : TargetedFunCommand
    {
        public Wine(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper) : base(source, channel, args, commandServiceHelper)
        {
        }

        protected override string TargetMessage
        {
            get { return "CmdWine"; }
        }
    }
}