using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Registration : GenericCommand
    {
        public Registration( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "registration" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            if( args.Length > 0 )
            {
                string userName = string.Join( " " , args );
                DateTime registrationDate = getRegistrationDate( userName );

                string[ ] messageParameters = { userName , registrationDate.ToString( "hh:mm:ss t" ) , registrationDate.ToString( "d MMMM yyyy" ) };
                string message = Configuration.Singleton().GetMessage("registrationDate", messageParameters);
                IAL.singleton.IrcPrivmsg( destination , message );
            }
            else
            {
                string[ ] messageParameters = { "registration" , "1" , args.Length.ToString( ) };
                IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );

            }
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
                DateTime regDate = DateTime.Parse( apiRegDate );
                return regDate;
            }
            else
            {
                return new DateTime( 0 );
            }

        }
    }
}
