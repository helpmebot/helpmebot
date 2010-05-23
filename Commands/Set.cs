using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Sets a global config option.
    /// </summary>
    class Set : GenericCommand
    {
        public Set( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );


            Configuration.Singleton( ).setOption( args[ 1 ] , args[ 0 ] , args[ 2 ] );
            return null;
        }
    }

}
