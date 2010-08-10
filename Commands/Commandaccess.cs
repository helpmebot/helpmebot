#region Usings

using System;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    internal class Commandaccess : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Type cmd =
                Type.GetType("helpmebot6.Commands." + args[0].Substring(0, 1).ToUpper() + args[0].Substring(1).ToLower());
            if ( cmd == null )
                return null;
            return
                new CommandResponseHandler(
                    ( (GenericCommand)Activator.CreateInstance( cmd ) ).
                        accessLevel.ToString( ) );
        }
    }
}