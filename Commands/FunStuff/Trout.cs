using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Trout  :GenericCommand
    {
        public Trout( )
        {

        }

        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            string name = string.Join( " ", args );

            string[] forbiddenTargets = { "stwalkerster", "itself", "himself", "herself", Helpmebot6.irc.IrcNickname.ToLower( ) };

            if( GlobalFunctions.isInArray(name.ToLower(),forbiddenTargets) != -1)
            {
                name = source.Nickname;
            }

            string[ ] messageparams = { name };
            string message = IAL.wrapCTCP( "ACTION", Configuration.Singleton( ).GetMessage( "cmdTrout", messageparams ) );

            return new CommandResponseHandler( message );

        }
    }
}
