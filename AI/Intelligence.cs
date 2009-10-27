using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.AI
{
    class Intelligence
    {
        private static Intelligence singleton;
        public Intelligence Singleton( )
        {
            if( singleton == null )
                singleton = new Intelligence( );

            return singleton;
        }

        protected Intelligence( )
        {

        }

        public string Respond( string input )
        {
            return ""; 
        }

    }
}
