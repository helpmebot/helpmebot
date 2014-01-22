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
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Monitoring
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Threading;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     Category watcher thread
    /// </summary>
    public class CategoryWatcher : IThreadedSystem
    {
        #region Fields

        /// <summary>
        ///     The category.
        /// </summary>
        private readonly string category;

        /// <summary>
        ///     The ignored pages repository.
        /// </summary>
        private readonly IIgnoredPagesRepository ignoredPagesRepository;

        /// <summary>
        ///     The key.
        /// </summary>
        private readonly string key;

        /// <summary>
        ///     The site.
        /// </summary>
        private readonly string site;

        /// <summary>
        ///     The sleep time.
        /// </summary>
        private int sleepTime = 180;

        /// <summary>
        ///     The watcher thread.
        /// </summary>
        private Thread watcherThread;

        #endregion

        #region Constructors and Destructors

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
            string baseWiki = LegacyConfig.Singleton()["baseWiki"];

            var q = new LegacyDatabase.Select("site_api");
            q.SetFrom("site");
            q.AddWhere(new LegacyDatabase.WhereConds("site_id", baseWiki));
            this.site = LegacyDatabase.Singleton().ExecuteScalarSelect(q);

            this.category = category;
            this.key = key;
            this.sleepTime = sleepTime;

            this.RegisterInstance();

            this.watcherThread = new Thread(this.WatcherThreadMethod);
            this.watcherThread.Start();

            // FIXME: servicelocator
            this.ignoredPagesRepository = ServiceLocator.Current.GetInstance<IIgnoredPagesRepository>();
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The category has items event.
        /// </summary>
        public event EventHandler<CategoryHasItemsEventArgs> CategoryHasItemsEvent;

        /// <summary>
        ///     The thread fatal error event.
        /// </summary>
        public event EventHandler ThreadFatalErrorEvent;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        ///     Gets or sets the time to sleep, in seconds.
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
                this.Log.Info("Restarting watcher...");
                this.watcherThread.Abort();
                Thread.Sleep(500);
                this.watcherThread = new Thread(this.WatcherThreadMethod);
                this.watcherThread.Start();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The do category check.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerable" />.
        /// </returns>
        public IEnumerable<string> DoCategoryCheck()
        {
            this.Log.Info("Getting items in category " + this.key);

            IEnumerable<string> pages = new List<string>();
            try
            {
                // Create the XML Reader
                string uri = this.site
                             + "?action=query&list=categorymembers&format=xml&cmlimit=50&cmprop=title&cmtitle="
                             + this.category;
                Stream xmlFragment = HttpRequest.Get(uri);

                XDocument xdoc = XDocument.Load(new StreamReader(xmlFragment));

                pages = from item in xdoc.Descendants("cm")
                        let xAttribute = item.Attribute("title")
                        where xAttribute != null
                        select xAttribute.Value;
            }
            catch (Exception ex)
            {
                this.Log.Error("Error contacting API (" + this.site + ") ", ex);
            }

            IEnumerable<string> pageList = pages;

            pageList = this.RemoveBlacklistedItems(pageList).ToList();

            return pageList;
        }

        /// <summary>
        ///     The get thread status.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string[] GetThreadStatus()
        {
            string[] statuses = { this.key + " " + this.watcherThread.ThreadState };
            return statuses;
        }

        /// <summary>
        ///     The register instance.
        /// </summary>
        public void RegisterInstance()
        {
            ThreadList.GetInstance().Register(this);
        }

        /// <summary>
        ///     The stop.
        /// </summary>
        public void Stop()
        {
            this.Log.Info("Stopping Watcher Thread for " + this.category + " ...");
            this.watcherThread.Abort();
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.key;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The remove blacklisted items.
        /// </summary>
        /// <param name="pageList">
        /// The page list.
        /// </param>
        /// <returns>
        /// The <see cref="List{String}"/>.
        /// </returns>
        private IEnumerable<string> RemoveBlacklistedItems(IEnumerable<string> pageList)
        {
            List<string> ignoredPages = this.ignoredPagesRepository.GetIgnoredPages().ToList();

            return pageList.Where(x => !ignoredPages.Contains(x));
        }

        /// <summary>
        ///     The watcher thread method.
        /// </summary>
        private void WatcherThreadMethod()
        {
            this.Log.Info("Starting category watcher for '" + this.key + "'...");
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

            this.Log.Warn("Category watcher for '" + this.key + "' died.");
        }

        #endregion
    }
}