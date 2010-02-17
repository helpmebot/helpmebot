/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/
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
                XmlTextReader xtr = new XmlTextReader(  HttpRequest.get(_apiUri + "?format=xml&noparse=true&action=fetch&id=" + id ));
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
                    return Configuration.Singleton().GetMessage( "fetchFaqTextNotFound", id.ToString( ) );
                }
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex  );
            }
            return null;
        }

        public string searchFaq( string searchTerm )
        {
            try
            {
                XmlTextReader xtr = new XmlTextReader(  HttpRequest.get(_apiUri + "?format=xml&action=search&noparse=true&query=" + searchTerm) );
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
                    return Configuration.Singleton().GetMessage( "fetchFaqTextNotFound", searchTerm );
                }
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex );
            }
            return null;
        }

        public string viewLink( int id )
        {
            return _apiUri.ToString( ).Replace( "api.php", "index.php?id=" + id.ToString( ) );
        }
    }
}
