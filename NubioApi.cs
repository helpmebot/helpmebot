using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
namespace helpmebot6
{
    /// <summary>
    /// Talks to the API for Nubio squared.
    /// </summary>
    public class NubioApi
    {
        Uri _apiUri; // http://stable.toolserver.org/nubio/api.php

        public NubioApi( Uri apiUri )
        {
            _apiUri = apiUri;
        }

        public string fetchFaqText( int id )
        {
            try
            {
                XmlTextReader xtr = new XmlTextReader( _apiUri + "?format=xml&noparse=true&action=fetch&id=" + id );
                xtr.WhitespaceHandling = WhitespaceHandling.None;

                xtr.Read( );
                try
                {
                    xtr.Read( );
                }
                catch ( System.Net.WebException )
                {
                }
                string text, title;
                text = title = "";
                while ( xtr.Read( ))
                {
                    if ( xtr.NodeType == XmlNodeType.Element )
                    {
                        if ( xtr.Name == "rev_text" )
                            text = System.Web.HttpUtility.HtmlDecode( Regex.Replace( xtr.ReadElementContentAsString( ).Replace( "\\", "" ), "<(.|\n)*?>", "" ) );
                        if ( xtr.Name == "page_title" )
                            title = System.Web.HttpUtility.HtmlDecode( Regex.Replace( xtr.ReadElementContentAsString( ).Replace( "\\", "" ), "<(.|\n)*?>", "" ) );
                    }
                }

                if ( text != "" && title != "" )
                {
                    return title + ": " + text;

                }
                else
                {
                    return Configuration.singleton.GetMessage( "fetchFaqTextNotFound", id.ToString( ) );
                }
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod());
            }
            return null;
        }

        public string searchFaq( string searchTerm )
        {
            try
            {
                XmlTextReader xtr = new XmlTextReader( _apiUri + "?format=xml&action=search&noparse=true&query=" + searchTerm );
                xtr.WhitespaceHandling = WhitespaceHandling.None;

                xtr.Read( );
                try
                {
                    xtr.Read( );
                }
                catch ( System.Net.WebException )
                {
                }

                string text, title;
                text = title = "";

                while ( xtr.Read( ) )
                {
                    if ( xtr.Name == "page_title" && xtr.NodeType == XmlNodeType.Element )
                    {
                        title = System.Web.HttpUtility.HtmlDecode( Regex.Replace( xtr.ReadElementContentAsString( ).Replace( "\\", "" ), "<(.|\n)*?>", "" ) );
                        text = System.Web.HttpUtility.HtmlDecode( Regex.Replace( xtr.ReadString( ).Replace( "\\", "" ), "<(.|\n)*?>", "" ) );
                    }
 
                }
                if ( text != "" && title != "")
                {
                    return title + ": " + text;
                }
                else
                {
                    return Configuration.singleton.GetMessage( "fetchFaqTextNotFound", searchTerm );
                }
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex, System.Reflection.MethodInfo.GetCurrentMethod( ) );
            }
            return null;
        }

        public string viewLink( int id )
        {
            return _apiUri.ToString( ).Replace( "api.php", "index.php?id=" + id.ToString( ) );
        }
    }
}
