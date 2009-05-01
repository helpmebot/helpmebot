using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
namespace helpmebot6
{
    /// <summary>
    /// Parses a category from MediaWiki
    /// </summary>
    public class MediaWikiCategoryParser
    {
        Uri _apiUri;
        string _category;

        public MediaWikiCategoryParser( Uri ApiUri, string Category )
        {
            _apiUri = ApiUri;
            _category = Category;
        }

        public ArrayList fetchCategoryMembers( int Limit )
        {
            ArrayList pages = new ArrayList( );
            try
            {
                //Create the XML Reader
                System.Xml.XmlTextReader xmlreader = new System.Xml.XmlTextReader( _apiUri+ "?action=query&list=categorymembers&format=xml&cmprop=title&cmtitle=" + _category );

                //Disable whitespace so that you don't have to read over whitespaces
                xmlreader.WhitespaceHandling = System.Xml.WhitespaceHandling.None;

                //read the xml declaration and advance to api tag
                xmlreader.Read( );
                //read the api tag
                xmlreader.Read( );
                //read the query tag
                xmlreader.Read( );
                //read the categorymembers tag
                xmlreader.Read( );

                while ( true )
                {
                    //Go to the name tag
                    xmlreader.Read( );

                    //if not start element exit while loop
                    if ( !xmlreader.IsStartElement( ) )
                    {
                        break;
                    }

                    //Get the title Attribute Value
                    string titleAttribute = xmlreader.GetAttribute( "title" );
                         pages.Add( titleAttribute );

                }

                //close the reader
                xmlreader.Close( );
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex, System.Reflection.MethodInfo.GetCurrentMethod( ) );
            }
            return pages;
        }

    }
}
