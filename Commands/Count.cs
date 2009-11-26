using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the edit count of a wikipedian
    /// </summary>
    class Count : GenericCommand
    {
        public Count( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel(  );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            if( args.Length > 0 )
            {
                string userName = string.Join( " " , args );
                int editCount = getEditCount( userName );
                if( editCount == -1 )
                {
                    string[ ] messageParams = { userName };
                    string message = Configuration.Singleton( ).GetMessage( "noSuchUser", messageParams );
                    return new CommandResponseHandler( message );
                }
                else
                {


                    string[ ] messageParameters = { editCount.ToString( ) , userName };

                    string message = Configuration.Singleton( ).GetMessage( "editCount" , messageParameters );

                    return new CommandResponseHandler( message );
                }
            }
            else
            {
                string[ ] messageParameters = { "count" , "1" , args.Length.ToString( ) };
                Helpmebot6.irc.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );
            }
            return null;
        }

        public int getEditCount( string username )
        {
            if( username == string.Empty )
            {
                throw new ArgumentNullException( );
            }

            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );

            string api = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_api` FROM `site` WHERE `site_id` = " + baseWiki + ";" );


            System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader( api + "?format=xml&action=query&list=users&usprop=editcount&format=xml&ususers=" + username );
            do
            {
                creader.Read( );
            } while( creader.Name != "user" );
            string editCount = creader.GetAttribute( "editcount" );
            if( editCount != null )
            {
                return int.Parse( editCount );
            }
            else
            {
                if( creader.GetAttribute( "missing" ) == "" )
                {
                    return -1;
                }
                else
                {
                    throw new ArgumentException( );
                }
            }


        }
    }
}
