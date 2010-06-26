#region Usings

using System;
using System.Collections;
using System.Reflection;

#endregion

namespace helpmebot6.Monitoring.PageWatcher
{
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

        private readonly IAL _irc;

        private readonly ArrayList _watchedPageList;

        public struct RcPageChange
        {
            public string title;
            public string flags;
            public string diffUrl;
            public string user;
            public string byteDiff;
            public string comment;
        }

        private void setupEvents()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            this._irc.connectionRegistrationSucceededEvent += irc_ConnectionRegistrationSucceededEvent;
            this._irc.privmsgEvent += irc_PrivmsgEvent;
            this.pageWatcherNotificationEvent += pageWatcherControllerPageWatcherNotificationEvent;
        }

        private static void pageWatcherControllerPageWatcherNotificationEvent(RcPageChange rcItem)
        {
        }

        public string[] getWatchedPages()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string[] wp = new string[this._watchedPageList.Count];
            this._watchedPageList.CopyTo(wp);
            return wp;
        }

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