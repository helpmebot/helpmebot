// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcher.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Category watcher thread
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Monitoring
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Threading;
    using System.Xml;

    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Threading;

    using log4net;

    /// <summary>
    /// Category watcher thread
    /// </summary>
    public class CategoryWatcher : IThreadedSystem
    {
        /// <summary>
        /// The log4net logger for this class
        /// </summary>
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _site;
        private readonly string _category;
        private readonly string _key;

        private Thread _watcherThread;

        private int _sleepTime = 180;


        public delegate void CategoryHasItemsEventHook(ArrayList items, string keyword);

        public event CategoryHasItemsEventHook categoryHasItemsEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryWatcher"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="key">The key.</param>
        /// <param name="sleepTime">The sleep time.</param>
        public CategoryWatcher(string category, string key, int sleepTime)
        {
            // look up site id
            string baseWiki = LegacyConfig.singleton()["baseWiki"];

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            this._site = DAL.singleton().executeScalarSelect(q);

            this._category = category;
            this._key = key;
            this._sleepTime = sleepTime;

            this.RegisterInstance();

            this._watcherThread = new Thread(this.watcherThreadMethod);
            this._watcherThread.Start();
        }

        private void watcherThreadMethod()
        {
            Log.Info("Starting category watcher for '" + this._key + "'...");
            try
            {
                while (true)
                {
                    Thread.Sleep(this.sleepTime*1000);
                    ArrayList categoryResults = this.doCategoryCheck();
                    if (categoryResults.Count > 0)
                    {
                        this.categoryHasItemsEvent(categoryResults, this._key);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                EventHandler temp = this.ThreadFatalErrorEvent;
                if (temp != null)
                {
                    temp(this, new EventArgs());
                }
            }
            Log.Warn("Category watcher for '" + this._key + "' died.");
        }


        /// <summary>
        ///   The time to sleep, in seconds.
        /// </summary>
        public int sleepTime
        {
            get { return this._sleepTime; }
            set
            {
                this._sleepTime = value;
                Log.Info("Restarting watcher...");
                this._watcherThread.Abort();
                Thread.Sleep(500);
                this._watcherThread = new Thread(this.watcherThreadMethod);
                this._watcherThread.Start();
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this._key;
        }

        /// <summary>
        /// Does the category check.
        /// </summary>
        /// <returns></returns>
        public ArrayList doCategoryCheck()
        {
            Log.Info("Getting items in category " + this._key);
            ArrayList pages = new ArrayList();
            try
            {
                //Create the XML Reader
                XmlTextReader xmlreader =
                    new XmlTextReader(
                        HttpRequest.get(this._site + "?action=query&list=categorymembers&format=xml&cmlimit=50&cmprop=title&cmtitle=" +
                                        this._category))
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
                Log.Error("Error contacting API (" + this._site + ") ", ex);
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

        public void RegisterInstance()
        {
            ThreadList.instance().register(this);
        }

        public void Stop()
        {
            Log.Info("Stopping Watcher Thread for " + this._category + " ...");
            this._watcherThread.Abort();
        }

        public string[] GetThreadStatus()
        {
            string[] statuses = {this._key + " " + this._watcherThread.ThreadState};
            return statuses;
        }

        public event EventHandler ThreadFatalErrorEvent;

        #endregion
    }
}