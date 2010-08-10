#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the number of messages that have been sent by the bot to IRC
    /// </summary>
    internal class Messagecount : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string[] messageParameters = {Helpmebot6.irc.messageCount.ToString()};
            return
                new CommandResponseHandler(Configuration.singleton().getMessage("messageCountReport", messageParameters));
        }
    }
}