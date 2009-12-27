using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the age of a wikipedian
    /// </summary>
    class Age  : GenericCommand
    {
        public Age( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            if( args.Length > 0 )
            {
                string username = string.Join( " " , args );
                TimeSpan time = getWikipedianAge( username );
                string message;
                if( time.Equals( new TimeSpan( 0 ) ) )
                {
                    string[ ] messageParameters = { username };
                    message = Configuration.Singleton( ).GetMessage( "noSuchUser" , messageParameters );
                }
                else
                {
                    string[ ] messageParameters = { username , ( time.Days / 365 ).ToString( ) , ( time.Days % 365 ).ToString( ) , time.Hours.ToString( ) , time.Minutes.ToString( ) , time.Seconds.ToString( ) };
                    message = Configuration.Singleton( ).GetMessage( "cmdAge" , messageParameters );
                }
                return new CommandResponseHandler( message );
            }
            else
            {
                string[ ] messageParameters = { "age" , "1" , args.Length.ToString( ) };
                Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );

            }
            return null;
        }

        public TimeSpan getWikipedianAge( string userName )
        {
            Registration regCommand = new Registration( );
            DateTime regdate = regCommand.getRegistrationDate( userName );
            TimeSpan age = DateTime.Now.Subtract( regdate );
            if( regdate.Equals( new DateTime( 0001 , 1 , 1 ) ) )
            {
                age = new TimeSpan( 0 );
            }
            return age;
        }
    }
}
