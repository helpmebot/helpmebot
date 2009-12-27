using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the block information of a wikipedian
    /// </summary>
    class Blockinfo : GenericCommand
    {
        public Blockinfo( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            return new CommandResponseHandler( getBlockInformation( string.Join(" ", args)  ).ToString( ) );
        }

        public BlockInformation getBlockInformation( string userName )
        {
           System.Net.IPAddress ip;

            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );
            string api = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_api` FROM `site` WHERE `site_id` = " + baseWiki + ";" );
            string apiParams = "?action=query&list=blocks&bk";
            if( System.Net.IPAddress.TryParse( userName, out ip ) )
            {
                apiParams += "ip";
            }
            else
            {
                apiParams += "users";
            }
            apiParams+= "="+userName+"&format=xml";
            System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader( api + apiParams );

            while( creader.Name != "blocks" )
            {
                creader.Read( );
            }
            creader.Read( );

            if( creader.Name == "block" )
            {
                BlockInformation bi = new BlockInformation( );
                bi.id = creader.GetAttribute( "id" );
                bi.target = creader.GetAttribute( "user" );
                bi.blockedBy = creader.GetAttribute( "by" );
                bi.start = creader.GetAttribute( "timestamp" );
                bi.expiry = creader.GetAttribute( "expiry" );
                bi.blockReason = creader.GetAttribute( "reason" );

                bi.autoblock = creader.GetAttribute( "autoblock" ) == "" ? true : false;
                bi.nocreate = creader.GetAttribute( "nocreate" ) == "" ? true : false;
                bi.noemail = creader.GetAttribute( "noemail" ) == "" ? true : false;
                bi.allowusertalk = creader.GetAttribute( "allowusertalk" ) == "" ? true : false;

                return bi;
            }
            else
            {
                return new BlockInformation( );
            }
        }

        public struct BlockInformation
        {
            public string id;
            public string target;
            public string blockedBy;
            public string blockReason;
            public string expiry;
            public string start;
            public bool nocreate;
            public bool autoblock;
            public bool noemail;
            public bool allowusertalk;

            public override string ToString( )
            {
                string[ ] emptyMessageParams = { "", "", "", "", "", "", "" };
                string emptyMessage = Configuration.Singleton( ).GetMessage( "blockInfoShort", emptyMessageParams );
                
                string info = "";
                if( nocreate )
                    info += "NOCREATE ";
                if( autoblock )
                    info += "AUTOBLOCK ";
                if( noemail )
                    info += "NOEMAIL ";
                if( allowusertalk )
                    info += "ALLOWUSERTALK ";
                string[ ] messageParams = { id, target, blockedBy, expiry, start, blockReason, info };
                string message = Configuration.Singleton( ).GetMessage( "blockInfoShort", messageParams );

                if( message == emptyMessage )
                {
                    message = Configuration.Singleton( ).GetMessage( "noBlocks" );
                }

                return message;
            }
        }
    }
}
