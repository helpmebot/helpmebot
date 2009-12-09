using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Autolink : GenericCommand
    {
        public Autolink( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel(  );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            bool global = false;


            if( args.Length > 0 )
            {
                if( args[ 0 ] == "@global" )
                {
                    global = true;
                    GlobalFunctions.popFromFront( ref args );
                }
            }
            bool oldValue;

            if( !global )
            {
                oldValue = bool.Parse(Configuration.Singleton().retrieveLocalStringOption("autoLink", channel));
            }
            else
            {
                oldValue = bool.Parse(Configuration.Singleton().retrieveGlobalStringOption("autoLink"));
            }

            if( args.Length > 0 )
            {
                string newValue = "global";
                switch( args[0] )
                {
                    case "enable":
                        newValue = "true";
                        break;
                    case "disable":
                        newValue = "false";
                        break;
                    case "global":
                        newValue = "global";
                        break;
                }
                if( newValue == oldValue.ToString().ToLower() )
                {
                    return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "no-change" ) , CommandResponseDestination.PRIVATE_MESSAGE );
                }
                else
                {
                    if( newValue == "global" )
                    {
                        Configuration.Singleton().deleteLocalOption("autoLink", channel);
                        return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "defaultConfig" ) , CommandResponseDestination.PRIVATE_MESSAGE );

                    }
                    else
                    {
                        if( !global )
                            Configuration.Singleton().setLocalOption("autoLink", channel, newValue);
                        else
                            Configuration.Singleton().setGlobalOption("autoLink", newValue);
                        return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) , CommandResponseDestination.PRIVATE_MESSAGE );

                    }
                }
            }
            string[] mP = { "autolink", 1.ToString(), args.Length.ToString() };
            return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "notEnoughParameters" , mP ) , CommandResponseDestination.PRIVATE_MESSAGE );
        }
    }
}
