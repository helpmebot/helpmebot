#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Kills the bot.
    /// </summary>
    internal class Die : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Helpmebot6.stop();
            return null;
        }
    }
}