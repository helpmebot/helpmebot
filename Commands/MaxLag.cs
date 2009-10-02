using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class MaxLag : GenericCommand
    {
        public MaxLag( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( "maxlag" );
        }

        protected override void execute( User source , string destination , string[ ] args )
        {


            string[ ] messageParameters = { source.Nickname , getMaxLag() };
            string message = Configuration.Singleton( ).GetMessage( "cmdMaxLag" , messageParameters );
            IAL.singleton.IrcPrivmsg( destination , message );
        }

        public string getMaxLag( )
        {
            // look up site id
            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );
            // get api
            string api = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_api` FROM `site` WHERE `site_id` = " + baseWiki + ";" );


            System.Xml.XmlTextReader mlreader = new System.Xml.XmlTextReader( api+ "?action=query&meta=siteinfo&siprop=dbrepllag&format=xml" );
            do
            {
                mlreader.Read( );
            } while( mlreader.Name != "db" );

            string lag = mlreader.GetAttribute( "lag" );

            return lag;

        }
    }
}
