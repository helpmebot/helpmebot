using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Myaccess : GenericCommand
    {
        public Myaccess( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            CommandResponseHandler crh = new CommandResponseHandler( );
            string[ ] cmdArgs = { source.ToString( ) , source.AccessLevel.ToString() };
            crh.respond( Configuration.Singleton( ).GetMessage( "cmdAccess" , cmdArgs ) );
            return crh;
        }
    }
}
