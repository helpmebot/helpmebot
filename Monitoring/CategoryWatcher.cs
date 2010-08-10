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

#region Usings

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Xml;
using helpmebot6.Threading;

#endregion

namespace helpmebot6.Monitoring
{
    public class CategoryWatcher : IThreadedSystem
    {
        private readonly string _site;
        private readonly string _category;
        private readonly string _key;

        private Thread _watcherThread;

        private int _sleepTime = 180;


        public delegate void CategoryHasItemsEventHook(ArrayList items, string keyword);

        public event CategoryHasItemsEventHook categoryHasItemsEvent;

        public CategoryWatcher(string category, string key, int sleepTime)
        {
            // look up site id
            string baseWiki = Configuration.singleton().retrieveGlobalStringOption("baseWiki");

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            _site = DAL.singleton().executeScalarSelect(q);

            _category = category;
            _key = key;
            _sleepTime = sleepTime;

            this.registerInstance();

            this._watcherThread = new Thread(watcherThreadMethod);
            this._watcherThread.Start();
        }

        private void watcherThreadMethod()
        {
            Logger.instance().addToLog("Starting category watcher for '" + _key + "'...", Logger.LogTypes.General);
            try
            {
                while (true)
                {
                    Thread.Sleep(this.sleepTime*1000);
                    ArrayList categoryResults = doCategoryCheck();
                    if (categoryResults.Count > 0)
                    {
                        this.categoryHasItemsEvent(categoryResults, _key);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                EventHandler temp = this.threadFatalError;
                if (temp != null)
                {
                    temp(this, new EventArgs());
                }
            }
            Logger.instance().addToLog("Category watcher for '" + _key + "' died.", Logger.LogTypes.Error);
        }


        /// <summary>
        ///   The time to sleep, in seconds.
        /// </summary>
        public int sleepTime
        {
            get { return _sleepTime; }
            set
            {
                _sleepTime = value;
                Logger.instance().addToLog("Restarting watcher...", Logger.LogTypes.Command);
                this._watcherThread.Abort();
                Thread.Sleep(500);
                this._watcherThread = new Thread(watcherThreadMethod);
                this._watcherThread.Start();
            }
        }

        public override string ToString()
        {
            return _key;
        }

        public ArrayList doCategoryCheck()
        {
            Logger.instance().addToLog("Getting items in category " + _key, Logger.LogTypes.General);
            ArrayList pages = new ArrayList();
            try
            {
                //Create the XML Reader
                XmlTextReader xmlreader =
                    new XmlTextReader(
                        HttpRequest.get(_site + "?action=query&list=categorymembers&format=xml&cmprop=title&cmtitle=" +
                                        _category))
                        {
                            WhitespaceHandling = WhitespaceHandling.None
                        };

                //Disable whitespace so that you don't have to read over whitespaces

                //read the xml declaration and advance to api tag
                xmlreader.Read();
                //read the api tag
                xmlreader.Read();
                //read the query tag
                xmlreader.Read();
                //read the categorymembers tag
                xmlreader.Read();

                while (true)
                {
                    //Go to the name tag
                    xmlreader.Read();

                    //if not start element exit while loop
                    if (!xmlreader.IsStartElement())
                    {
                        break;
                    }

                    //Get the title Attribute Value
                    string titleAttribute = xmlreader.GetAttribute("title");
                    pages.Add(titleAttribute);
                }

                //close the reader
                xmlreader.Close();
            }
            catch (Exception ex)
            {
                Logger.instance().addToLog("Error contacting API (" + _site + ") " + ex.Message, Logger.LogTypes.DNWB);
            }
            pages = removeBlacklistedItems(pages);

            return pages;
        }

        private static ArrayList removeBlacklistedItems(ArrayList pageList)
        {
            DAL.Select q = new DAL.Select("ip_title");
            q.setFrom("ignoredpages");
            ArrayList blacklist = DAL.singleton().executeSelect(q);

            foreach (object[] item in blacklist)
            {
                if (pageList.Contains(item[0]))
                {
                    pageList.Remove(item[0]);
                }
            }

            return pageList;
        }

        #region IThreadedSystem Members

        public void registerInstance()
        {
            ThreadList.instance().register(this);
        }

        public void stop()
        {
            Logger.instance().addToLog("Stopping Watcher Thread for " + _category + " ...", Logger.LogTypes.General);
            this._watcherThread.Abort();
        }

        public string[] getThreadStatus()
        {
            string[] statuses = {_key + " " + this._watcherThread.ThreadState};
            return statuses;
        }

        public event EventHandler threadFatalError;

        #endregion
    }
}