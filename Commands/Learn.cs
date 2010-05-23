using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Learns a keyword
    /// </summary>
    class Learn : GenericCommand
    {
        public Learn( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            bool action = false;
            if( args[ 0 ] == "@action" )
            {
                action = true;
                GlobalFunctions.popFromFront( ref args );
            }

            if( args.Length >= 2 )
            {
               if( WordLearner.Learn( args[ 0 ] , string.Join( " " , args , 1 , args.Length - 1 ) , action))
                   Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "cmdLearnDone"  ) );
               else
                   Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "cmdLearnError"  ) );
 
            }
            else
            {
                string[ ] messageParameters = { "learn" , "2" , args.Length.ToString( ) };
                Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );
            }
            return null;
        }
    }
}
