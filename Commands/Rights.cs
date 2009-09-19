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

            IAL.singleton.IrcPrivmsg( destination , message );

        }



        string getRights( string username )
        {
            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );
            MySql.Data.MySqlClient.MySqlDataReader dr = DAL.Singleton( ).ExecuteReaderQuery( "SELECT `site_mainpage`, `site_username`, `site_password` FROM `site` WHERE `site_id` = "+baseWiki+";" );
            dr.Read( );
            string[] vals = new string[3];
            dr.GetValues( vals );
            dr.Close( );
            GlobalFunctions.Log( "%% DNWB" );
            DotNetWikiBot.Site mainWiki = new DotNetWikiBot.Site( vals[ 0 ] , vals[ 1 ] , vals[ 2 ] );
            GlobalFunctions.Log( "%% DNWB END" );

            string returnStr = "";
            string rightsList;
            int rightsCount = 0;
            System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader( mainWiki.site + mainWiki.indexPath + "api.php" + "?action=query&list=users&usprop=groups&format=xml&ususers=" + username );
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
