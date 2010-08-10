#region Usings
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Tweet : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string status = string.Join(" ", args);

            new Twitter().updateStatus(status);
            return new CommandResponseHandler( Configuration.singleton( ).getMessage( "done" ) );
        }
    }
}