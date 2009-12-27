using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    class Disable: GenericCommand
    {
        public Disable( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Monitoring.WatcherController.Instance( ).removeWatcherFromChannel( args[ 0 ] , channel );
            return null;
        }
    }
}
