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
            if( args.Length > 0 )
            {
                string username = string.Join( " " , args );
                TimeSpan time = getWikipedianAge( username );
                string[ ] messageParameters = { username , time.Days , time.Hours , time.Minutes , time.Seconds };
                string message = Configuration.Singleton( ).GetMessage( "cmdAge" , messageParameters );
                IAL.singleton.IrcPrivmsg( destination , message );
            }
            else
            {
                string[ ] messageParameters = { "age" , "1" , args.Length.ToString( ) };
                IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );

            }
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
