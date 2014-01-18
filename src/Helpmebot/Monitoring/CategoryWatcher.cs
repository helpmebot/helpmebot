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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Xml;

    using Castle.Core.Logging;

    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Threading;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Category watcher thread
    /// </summary>
    public class CategoryWatcher : IThreadedSystem
    {
        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        /// The site.
        /// </summary>
        private readonly string site;

        /// <summary>
        /// The category.
        /// </summary>
        private readonly string category;

        /// <summary>
        /// The key.
        /// </summary>
        private readonly string key;

        /// <summary>
        /// The watcher thread.
        /// </summary>
        private Thread watcherThread;

        /// <summary>
        /// The sleep time.
        /// </summary>
        private int sleepTime = 180;

        /// <summary>
        /// Initialises a new instance of the <see cref="CategoryWatcher"/> class.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="sleepTime">
        /// The sleep time.
        /// </param>
        public CategoryWatcher(string category, string key, int sleepTime)
        {
            // FIXME: Remove me!
            this.Log = ServiceLocator.Current.GetInstance<ILogger>();

            // look up site id
            string baseWiki = LegacyConfig.singleton()["baseWiki"];

            LegacyDatabase.Select q = new LegacyDatabase.Select("site_api");
            q.SetFrom("site");
            q.AddWhere(new LegacyDatabase.WhereConds("site_id", baseWiki));
            this.site = LegacyDatabase.Singleton().ExecuteScalarSelect(q);

            this.category = category;
            this.key = key;
            this.sleepTime = sleepTime;

            this.RegisterInstance();

            this.watcherThread = new Thread(this.WatcherThreadMethod);
            this.watcherThread.Start();
        }

        /// <summary>
        /// The category has items event.
        /// </summary>
        public event EventHandler<CategoryHasItemsEventArgs> CategoryHasItemsEvent;

        /// <summary>
        /// The thread fatal error event.
        /// </summary>
        public event EventHandler ThreadFatalErrorEvent;

        /// <summary>
        ///  Gets or sets the time to sleep, in seconds.
        /// </summary>
        public int SleepTime
        {
            get
            {
                return this.sleepTime;
            }

            set
            {
                this.sleepTime = value;
                Log.Info("Restarting watcher...");
                this.watcherThread.Abort();
                Thread.Sleep(500);
                this.watcherThread = new Thread(this.WatcherThreadMethod);
                this.watcherThread.Start();
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
            return this.key;
        }

        /// <summary>
        /// The register instance.
        /// </summary>
        public void RegisterInstance()
        {
            ThreadList.GetInstance().Register(this);
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public void Stop()
        {
            Log.Info("Stopping Watcher Thread for " + this.category + " ...");
            this.watcherThread.Abort();
        }

        /// <summary>
        /// The get thread status.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string[] GetThreadStatus()
        {
            string[] statuses = { this.key + " " + this.watcherThread.ThreadState };
            return statuses;
        }

        /// <summary>
        /// Does the category check.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<string> DoCategoryCheck()
        {
            Log.Info("Getting items in category " + this.key);
            List<string> pages = new List<string>();
            try
            {
                // Create the XML Reader
                XmlTextReader xmlreader =
                    new XmlTextReader(
                        HttpRequest.get(this.site + "?action=query&list=categorymembers&format=xml&cmlimit=50&cmprop=title&cmtitle=" +
                                        this.category))
                    {
                        // Disable whitespace so that you don't have to read over whitespaces
                        WhitespaceHandling = WhitespaceHandling.None
                    };

                // read the xml declaration and advance to api tag
                xmlreader.Read();

                // read the api tag
                xmlreader.Read();

                // read the query tag
                xmlreader.Read();

                // read the categorymembers tag
                xmlreader.Read();

                while (true)
                {
                    // Go to the name tag
                    xmlreader.Read();

                    // if not start element exit while loop
                    if (!xmlreader.IsStartElement())
                    {
                        break;
                    }

                    // Get the title Attribute Value
                    string titleAttribute = xmlreader.GetAttribute("title");
                    pages.Add(titleAttribute);
                }

                // close the reader
                xmlreader.Close();
            }
            catch (Exception ex)
            {
                Log.Error("Error contacting API (" + this.site + ") ", ex);
            }

            pages = RemoveBlacklistedItems(pages);

            return pages;
        }

        /// <summary>
        /// The remove blacklisted items.
        /// </summary>
        /// <param name="pageList">
        /// The page list.
        /// </param>
        /// <returns>
        /// The <see cref="List{String}"/>.
        /// </returns>
        private static List<string> RemoveBlacklistedItems(List<string> pageList)
        {
            LegacyDatabase.Select q = new LegacyDatabase.Select("ip_title");
            q.SetFrom("ignoredpages");
            ArrayList blacklist = LegacyDatabase.Singleton().ExecuteSelect(q);

            foreach (object[] item in blacklist)
            {
                if (pageList.Contains((string)item[0]))
                {
                    pageList.Remove((string)item[0]);
                }
            }

            return pageList;
        }

        /// <summary>
        /// The watcher thread method.
        /// </summary>
        private void WatcherThreadMethod()
        {
            Log.Info("Starting category watcher for '" + this.key + "'...");
            try
            {
                while (true)
                {
                    Thread.Sleep(this.SleepTime * 1000);
                    IEnumerable<string> categoryResults = this.DoCategoryCheck().ToList();
                    if (categoryResults.Any())
                    {
                        this.CategoryHasItemsEvent(this, new CategoryHasItemsEventArgs(categoryResults, this.key));
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

            Log.Warn("Category watcher for '" + this.key + "' died.");
        }
    }
}