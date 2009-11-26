using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the registration date of a wikipedian
    /// </summary>
    class Registration : GenericCommand
    {
        public Registration( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel(  );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            CommandResponseHandler crh = new CommandResponseHandler( );
            if( args.Length > 0 )
            {
                string userName = string.Join( " " , args );
                DateTime registrationDate = getRegistrationDate( userName );
                if( registrationDate == new DateTime( 0 ) )
                {
                    string[ ] messageParams = { userName };
                    string message = Configuration.Singleton( ).GetMessage( "noSuchUser" , messageParams );
                    crh.respond( message );
                }
                else
                {


                    string[ ] messageParameters = { userName , registrationDate.ToString( "hh:mm:ss t" ) , registrationDate.ToString( "d MMMM yyyy" ) };
                    string message = Configuration.Singleton( ).GetMessage( "registrationDate" , messageParameters );
                    crh.respond( message );
                }
            }
            else
            {
                string[ ] messageParameters = { "registration" , "1" , args.Length.ToString( ) };
                Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );

            }
            return crh;
        }

        public DateTime getRegistrationDate( string username )
        {
            if( username == string.Empty )
            {
                throw new ArgumentNullException( );
            }
            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );

            string api = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_api` FROM `site` WHERE `site_id` = " + baseWiki + ";" );

            System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader( api + "?action=query&list=users&usprop=registration&format=xml&ususers=" + username );
            do
            {
                creader.Read( );
            } while( creader.Name != "user" );
            string apiRegDate = creader.GetAttribute( "registration" );
            if( apiRegDate != null )
            {
                if( apiRegDate == "" )
                {
                    return new DateTime( 1970 , 1 , 1 , 0 , 0 , 0 );
                }
                else
                {
                    DateTime regDate = DateTime.Parse( apiRegDate );
                    return regDate;
                }
            }
            else
            {
                return new DateTime( 0 );
            }

        }
    }

    /// <summary>
    /// Returns the registration date of a wikipedian. Alias for Registration
    /// </summary>
    class Reg : Registration
    {

    }
}
