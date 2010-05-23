using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Isgd:GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            return new CommandResponseHandler( IsGd.shorten( new Uri( args[ 0 ] ) ).ToString( ) );
        }
    }
}
