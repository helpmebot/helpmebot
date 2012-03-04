using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using helpmebot6.Threading;

namespace helpmebot6.Monitoring
{
    class AccNotifications : IThreadedSystem
    {
        private Thread _watcherThread;

        private static AccNotifications instance;

        public static AccNotifications getInstance()
        {
            if(instance == null)
            {
                instance = new AccNotifications();

            }
            return instance;
        }


        private AccNotifications()
        {
            Helpmebot6.irc.ircPrivmsg("##helpmebot", "ACC Notifications watcher initialising");
            _watcherThread = new Thread(threadBody);
            _watcherThread.Start();
	        this.registerInstance();
        }

        private void threadBody()
        {
            Helpmebot6.irc.ircPrivmsg("##helpmebot", "ACC Notifications watcher starting");
            Logger.instance().addToLog("Starting ACC Notifications watcher", Logger.LogTypes.General);
            try
            {
                DAL.Select query = new DAL.Select("notif_id", "notif_text");
                query.setFrom("acc_notifications");
                query.addLimit(1, 0);
                query.addOrder(new DAL.Select.Order("notif_id", true));

                while (true)
                {
                    Thread.Sleep(5000);

                    ArrayList data = DAL.singleton().executeSelect(query);

                    if (data.Count == 0)
                    {
                        continue;
                    }

                    foreach (object raw in data)
                    {
                        object[] d = (object[]) raw;
                        int id = (int) d[0];
                        string text = (string) d[1];

                        DAL.singleton().delete("acc_notifications", 1, new DAL.WhereConds("notif_id", id));
                        Helpmebot6.irc.ircPrivmsg("#wikipedia-en-accounts", text);
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
            Helpmebot6.irc.ircPrivmsg("##helpmebot", "ACC Notifications watcher died");
            Logger.instance().addToLog("ACC Notifications watcher died.", Logger.LogTypes.Error);
        }

        #region Implementation of IThreadedSystem

        /// <summary>
        ///   Stop all threads in this instance to allow for a clean shutdown.
        /// </summary>
        public void stop()
        {
            Logger.instance().addToLog("Stopping ACC Notifications thread...", Logger.LogTypes.General);
            this._watcherThread.Abort();
        }

        /// <summary>
        ///   Register this instance of the threaded class with the global list
        /// </summary>
        public void registerInstance()
        {
            ThreadList.instance().register(this);
        }

        /// <summary>
        ///   Get the status of thread(s) in this instance.
        /// </summary>
        /// <returns></returns>
        public string[] getThreadStatus()
        {
            string[] statuses = { this._watcherThread.ThreadState.ToString() };
            return statuses;
        }

        public event EventHandler threadFatalError;

        #endregion
    }
}
