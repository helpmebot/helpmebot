using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Age  : GenericCommand
    {
        public Age( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "age" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            throw new NotImplementedException( );
        }

        public TimeSpan getWikipedianAge( string userName )
        {
            Registration regCommand = new Registration( );
            DateTime regdate = regCommand.getRegistrationDate( userName );
            TimeSpan age = DateTime.Now.Subtract( regdate );
            return age;
        }
    }
}
