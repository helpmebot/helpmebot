namespace helpmebot6.Commands
{
    internal class Helper : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            // FIXME: this needs putting into its own subsystem, messageifying, configifying, etc.
            if (channel == "#wikipedia-en-help")
            {
                string message = "[HELP]: " + source + " needs help in #wikipedia-en-help!";
                if (args.Length > 0)
                    message += " (message: \"" + string.Join(" ", args) + "\")";

                Helpmebot6.irc.ircNotice("#wikipedia-en-helpers", message);
            }
            return null;
        }
    }
}