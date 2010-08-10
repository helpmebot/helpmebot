#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Raw : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Helpmebot6.irc.sendRawLine(string.Join(" ", args));

            return new CommandResponseHandler();
        }
    }
}