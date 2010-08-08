// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System;
using System.Collections;
using System.Reflection;

#endregion

namespace helpmebot6.Monitoring.PageWatcher
{
    /// <summary>
    /// Controller class for the pagewatcher
    /// </summary>
    internal class PageWatcherController
    {
        #region woo singleton

        private static PageWatcherController _instance;

        protected PageWatcherController()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            this._watchedPageList = new ArrayList();
            if ( !Helpmebot6.pagewatcherEnabled ) return;
            this.loadAllWatchedPages();
            uint wikiRCIrc = Configuration.singleton().retrieveGlobalUintOption("wikimediaRcNetwork");
            if ( wikiRCIrc == 0 ) return;
            this._irc = new IAL(wikiRCIrc);
            this.setupEvents();
            this._irc.connect();
        }

        public static PageWatcherController instance()
        {
            return _instance ?? ( _instance = new PageWatcherController( ) );
        }
        #endregion

        /// <summary>
        /// Holds the connection object to browne.
        /// </summary>
        private readonly IAL _irc;

        /// <summary>
        /// list of watched pages
        /// </summary>
        private readonly ArrayList _watchedPageList;
        
        /// <summary>
        /// Structure to hold the information about a page change recieved from browne.
        /// </summary>
        public struct RcPageChange
        {
            public string title;
            public string flags;
            public string diffUrl;
            public string user;
            public string byteDiff;
            public string comment;
        }


        /// <summary>
        /// Setups the events for the browne IRC access layer.
        /// </summary>
        private void setupEvents()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            this._irc.connectionRegistrationSucceededEvent += irc_ConnectionRegistrationSucceededEvent;
            this._irc.privmsgEvent += irc_PrivmsgEvent;
            this.pageWatcherNotificationEvent += pageWatcherControllerPageWatcherNotificationEvent;
        }

        // TODO: what's the point in this function? is it to prevent a nullref on the event calls?
        private static void pageWatcherControllerPageWatcherNotificationEvent(RcPageChange rcItem)
        {
        }

        /// <summary>
        /// Gets the watched pages.
        /// </summary>
        /// <returns></returns>
        public string[] getWatchedPages()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string[] wp = new string[this._watchedPageList.Count];
            this._watchedPageList.CopyTo(wp);
            return wp;
        }

        /// <summary>
        /// Loads all watched pages from the database.
        /// </summary>
        public void loadAllWatchedPages()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            this._watchedPageList.Clear();
            DAL.Select q = new DAL.Select("pw_title");
            q.setFrom("watchedpages");
            ArrayList pL = DAL.singleton().executeSelect(q);
            foreach (object[] item in pL)
            {
                this._watchedPageList.Add(item[0]);
            }
        }

        /// <summary>
        /// browne IRC PRIVMSG event handler
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="message">The message.</param>
        private void irc_PrivmsgEvent(User source, string destination, string message)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (source.ToString() == Configuration.singleton().retrieveGlobalStringOption("wikimediaRcBot"))
            {
                RcPageChange rcItem = rcParser(message);

                // not a page edit
                if (rcItem.title == string.Empty)
                    return;

                if (this._watchedPageList.Contains(rcItem.title))
                {
                    this.pageWatcherNotificationEvent(rcItem);
                }
            }
        }

        /// <summary>
        /// browne IRC connection registration succeeded event handler
        /// </summary>
        private void irc_ConnectionRegistrationSucceededEvent()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            uint network = Configuration.singleton().retrieveGlobalUintOption("wikimediaRcNetwork");

            DAL.Select q = new DAL.Select("channel_name");
            q.setFrom("channel");
            q.addWhere(new DAL.WhereConds("channel_enabled", 1));
            q.addWhere(new DAL.WhereConds("channel_network", network.ToString()));
            foreach (object[] item in DAL.singleton().executeSelect(q))
            {
                this._irc.ircJoin((string) (item[0]));
            }
        }

        /// <summary>
        /// Parses a line from the RC bot.
        /// </summary>
        /// <param name="rcItem">The rc item.</param>
        /// <returns></returns>
        private static RcPageChange rcParser(string rcItem)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            const string colorCodeControlChar = "\x03";
            string[] colorCodes = {
                                      colorCodeControlChar + "4",
                                      colorCodeControlChar + "5",
                                      colorCodeControlChar + "07",
                                      colorCodeControlChar + "10",
                                      colorCodeControlChar + "14",
                                      colorCodeControlChar + "02",
                                      colorCodeControlChar + "03",
                                      colorCodeControlChar
                                  };

            string[] parts = rcItem.Split(colorCodes, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 12)
            {
                return new RcPageChange();
            }
            if (parts[1].Contains("Special:"))
            {
                return new RcPageChange();
            }

            RcPageChange ret = new RcPageChange
                                   {
                                       title = parts[ 1 ],
                                       flags = parts[ 3 ].Trim( ),
                                       diffUrl = parts[ 5 ],
                                       user = parts[ 9 ],
                                       byteDiff = parts[ 12 ].Trim( '(', ')' )
                                   };
            if (parts.Length > 13)
            {
                ret.comment = parts[13];
            }
            return ret;
        }

        /// <summary>
        /// Watches a page.
        /// </summary>
        /// <param name="pageName">Name of the page.</param>
        public void watchPage(string pageName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            // addOrder to database
            DAL.singleton().insert("watchedpages", "", pageName);
            // addOrder to arraylist
            this._watchedPageList.Add(pageName);
        }

        /// <summary>
        /// Unwatches a page.
        /// </summary>
        /// <param name="pageName">Name of the page.</param>
        public void unwatchPage(string pageName)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            //remove from database
            DAL.singleton().delete("watchedpages", 0, new DAL.WhereConds("pw_title", pageName));
            // remove from arraylist
            this._watchedPageList.Remove(pageName);
        }

        public delegate void PageWatcherNotificationEventDelegate(RcPageChange rcItem);

        public event PageWatcherNotificationEventDelegate pageWatcherNotificationEvent;
    }
}