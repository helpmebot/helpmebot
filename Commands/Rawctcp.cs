using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Rawctcp : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            string cmd = GlobalFunctions.popFromFront( ref args );
            string dst = GlobalFunctions.popFromFront( ref args );

            Helpmebot6.irc.IrcPrivmsg( dst, IAL.wrapCTCP( cmd, string.Join( " ", args ) ) );

            return null;
        }
    }
}
