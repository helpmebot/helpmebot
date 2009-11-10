using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Returns the block information of a wikipedian
    /// </summary>
    class BlockInfo : GenericCommand
    {
        public BlockInfo( )
        {
            accessLevel = GlobalFunctions.commandAccessLevel( );
        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            throw new NotImplementedException( );
        }

        //bool isBlocked( string username )
        //{
        //    try
        //    {
        //        System.Xml.XmlTextReader creader = new System.Xml.XmlTextReader(
        //                "http://en.wikipedia.org/w/api.php?format=xml&action=query&list=users&usprop=blockinfo&ususers=" + username );
        //        do
        //        {
        //            creader.Read( );
        //        } while( creader.Name != "user" );
        //        string missing = creader.GetAttribute( "missing" );
        //        if( missing == null )
        //        {
        //            string blockedby = creader.GetAttribute( "blockedby" );
        //            if( blockedby == null )
        //            {
        //                return false;
        //            }
        //            else
        //            {
        //                return true;
        //            }
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch( Exception ex )
        //    {
        //        sendMessage( debugIrcChannel , "***ERROR***: " + ex.ToString( ) + ": " + ex.Message , true , false );
        //    }
        //    return false;
        //}
    }
}
