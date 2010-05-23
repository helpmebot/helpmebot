using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Raw : GenericCommand
    {
        public Raw()
        {
        }

        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            Helpmebot6.irc.SendRawLine(string.Join(" ", args));

            return new CommandResponseHandler();
        }

    }
}
