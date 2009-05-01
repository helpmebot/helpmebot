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

namespace helpmebot6
{
    public class CategoryWatcher
    {

        Uri _apiUri;
        string _category;

        Thread watcherThread;

        int _sleepTime;


        public delegate void CategoryHasItemsEventDelegate( ArrayList items );
        public event CategoryHasItemsEventDelegate CategoryHasItemsEvent;

        

        public CategoryWatcher( string Category, Uri ApiUrl )
        {
            _apiUri = ApiUrl;
            _category = Category;

            watcherThread = new Thread( new ThreadStart( this.watcherThreadMethod ) );
            watcherThread.Start( );
           
        }

        private void watcherThreadMethod( )
        {
            try
            {
                while ( true )
                {
                    ArrayList categoryResults = this.doCategoryCheck( );
                    if ( categoryResults.Count > 0 )
                    {
                        CategoryHasItemsEvent( categoryResults );
                    }
                    Thread.Sleep( this.SleepTime );
                }
            }
            catch ( ThreadAbortException ex )
            {
                GlobalFunctions.ErrorLog( ex, System.Reflection.MethodInfo.GetCurrentMethod( ) );
            }
        }

        private ArrayList doCategoryCheck( )
        {
            return new ArrayList();
        }


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

    }
}
