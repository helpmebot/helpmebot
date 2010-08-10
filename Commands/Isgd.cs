#region Usings

using System;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Isgd : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            return new CommandResponseHandler(IsGd.shorten(new Uri(args[0])).ToString());
        }
    }
}