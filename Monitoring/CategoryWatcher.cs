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
using System.Threading;
using System.Collections;
using helpmebot6.Threading;

namespace helpmebot6.Monitoring
{
    public class CategoryWatcher : IThreadedSystem
    {

        string _site;
        string _category;
        string _key;

        Thread watcherThread;

        int _sleepTime = 180;


        public delegate void CategoryHasItemsEventHook( ArrayList items, string keyword);
        public event CategoryHasItemsEventHook CategoryHasItemsEvent;

        public CategoryWatcher( string Category, string Key, int SleepTime )
        {
            // look up site id
            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );
            _site = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_api` FROM `site` WHERE `site_id` = " + baseWiki + ";" );
            _category = Category;
            _key = Key;
            _sleepTime = SleepTime;

            RegisterInstance( );

            watcherThread = new Thread( new ThreadStart( this.watcherThreadMethod ) );
            watcherThread.Start( );
           
        }

        private void watcherThreadMethod( )
        {
            Logger.Instance( ).addToLog( "Starting category watcher for '" + _key + "'..." , Logger.LogTypes.GENERAL );
            try
            {
                while ( true )
                {
                    Thread.Sleep( this.SleepTime * 1000 );
                    ArrayList categoryResults = this.doCategoryCheck( );
                    if ( categoryResults.Count > 0)
                    {
                        CategoryHasItemsEvent( categoryResults, _key);
                    }
                }
            }
            catch ( ThreadAbortException ex )
            {
                GlobalFunctions.ErrorLog( ex );
            }
            Logger.Instance( ).addToLog( "Category watcher for '" + _key + "' died." , Logger.LogTypes.ERROR );
        }



        /// <summary>
        /// The time to sleep, in seconds.
        /// </summary>
        public int SleepTime
        {
            get
            {
                return _sleepTime;
            }
            set
            {
                _sleepTime = value;
            }
        }

        public override string ToString( )
        {
            return _key;
        }

        public ArrayList doCategoryCheck( )
        {
            Logger.Instance( ).addToLog( "Getting items in category " + _key , Logger.LogTypes.GENERAL);
            ArrayList pages = new ArrayList( );
            try
            {
                //Create the XML Reader
                System.Xml.XmlTextReader xmlreader = new System.Xml.XmlTextReader(  HttpRequest.get(_site + "?action=query&list=categorymembers&format=xml&cmprop=title&cmtitle=" + _category ));

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

                while( true )
                {
                    //Go to the name tag
                    xmlreader.Read( );

                    //if not start element exit while loop
                    if( !xmlreader.IsStartElement( ) )
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
            catch( Exception ex )
            {
                Logger.Instance( ).addToLog( "Error contacting API (" + _site + ") " + ex.Message , Logger.LogTypes.DNWB );
            }
            return pages;

        }


        #region IThreadedSystem Members

        public void RegisterInstance( )
        {
            ThreadList.instance( ).register( this );
        }

        public void Stop( )
        {
            Logger.Instance( ).addToLog( "Stopping Watcher Thread for " + _category + " ...", Logger.LogTypes.GENERAL );
            watcherThread.Abort( );
        }

        public string[ ] getThreadStatus( )
        {
            throw new NotImplementedException( );
        }
        #endregion
    }
}
