using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class CategoryWatcher : GenericCommand
    {
        public CategoryWatcher( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "categorywatcher" );

        }

        protected override CommandResponseHandler execute( User source , string[ ] args )
        {
            return new CommandResponseHandler( );
        }
        
    }
}
