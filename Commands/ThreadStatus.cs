using System;
using System.Collections.Generic;
using System.Text;
using helpmebot6.Threading;

namespace helpmebot6.Commands
{
    class Threadstatus : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            string[ ] statuses = ThreadList.instance( ).getAllThreadStatus( );
            CommandResponseHandler crh = new CommandResponseHandler( );
            foreach( string item in statuses )
            {
                crh.respond( item );
            }
            return crh;
        }
    }
}
