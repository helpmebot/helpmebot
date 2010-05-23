using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Link : GenericCommand
    {
        public Link( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            bool secure = bool.Parse(Configuration.Singleton().retrieveLocalStringOption("useSecureWikiServer", channel));
            if (args.Length > 0)
            {
                if (args[0] == "@secure")
                {
                    secure = true;
                    GlobalFunctions.popFromFront(ref args);
                }
            }

            string key = channel;
            if( GlobalFunctions.RealArrayLength(args) > 0 )
            {
                key = "<<<REALTIME>>>";
                Linker.Instance( ).ParseMessage( string.Join( " " , args ) , key );
            }

            return new CommandResponseHandler(Linker.Instance().GetLink(key, secure));
        }
    }
}
