using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Rights : GenericCommand
    {
        public Rights( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "rights" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {
            if( args.Length > 0 )
            {

                string username = string.Join( " " , args );
                string rights = getRights( username );


                string message = "";
                if( rights != "" )
                {
                    string[ ] messageParameters = { username , rights };
                    message = Configuration.Singleton( ).GetMessage( "cmdRightsList" , messageParameters );

                }
                else
                {
                    message = Configuration.Singleton( ).GetMessage( "cmdRightsNone" , username );
                }

                Helpmebot6.irc.IrcPrivmsg( destination , message );
            }
            else
            {
                string[ ] messageParameters = { "rights" , "1" , args.Length.ToString() };
                
                Helpmebot6.irc.IrcNotice( source.Nickname ,Configuration.Singleton( ).GetMessage( "notEnoughParameters" , messageParameters ) );
            }
        }



       public string getRights( string username )
        {
            if( username == string.Empty )
            {
                throw new ArgumentNullException( );
            }
            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );
       
            string api = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_api` FROM `site` WHERE `site_id` = " + baseWiki + ";" );


            string returnStr = "";
            string rightsList;
            int rightsCount = 0;
            System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader( api + "?action=query&list=users&usprop=groups&format=xml&ususers=" + username );
            do
                creader.Read( );
            while( creader.Name != "user" );
            creader.Read( );
            if( creader.Name == "groups" ) //the start of the group list
            {
                do
                {
                    creader.Read( );
                    rightsList = ( creader.ReadString( ) );
                    if( rightsList != "" )
                        returnStr = returnStr + rightsList + ", ";
                    rightsCount = rightsCount + 1;
                }
                while( creader.Name == "g" ); //each group should be added
            }
            if( rightsCount == 0 )
                returnStr = "";
            else
                returnStr = returnStr.Remove( returnStr.Length - 2 );


            return returnStr;
        }
    }
}
