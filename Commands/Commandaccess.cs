using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Commandaccess : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );


            Type cmd = Type.GetType( "helpmebot6.Commands." + args[ 0 ].Substring(0,1).ToUpper() + args[0].Substring(1).ToLower() );
            if( cmd != null )
                return new CommandResponseHandler( ( (GenericCommand)Activator.CreateInstance( cmd ) ).accessLevel.ToString( ) );
            else
                return null;
        }
    }
}
