﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThreadList.cs" company="Helpmebot Development Team">
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
//   Maintains a list of all the available threads the bot is running
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Threading
{
    using System;
    using System.Collections;
    using System.Threading;

    /// <summary>
    /// Maintains a list of all the available threads the bot is running
    /// </summary>
    internal class ThreadList
    {
        private static ThreadList _instance;

        public static ThreadList instance()
        {
            return _instance ?? ( _instance = new ThreadList( ) );
        }

        protected ThreadList()
        {
            this._threadedObjects = new ArrayList();
        }

        private readonly ArrayList _threadedObjects;
        public ArrayList ThreadedObjects { get { return _threadedObjects; } }

        /// <summary>
        /// Registers the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public void register(IThreadedSystem sender)
        {
            this._threadedObjects.Add(sender);
        }

        /// <summary>
        /// Creates a new thread to start shutting down other threads
        /// </summary>
        public void stop()
        {
            Thread shutdownControllerThread
                = new Thread(this.shutdownMethod);

            shutdownControllerThread.Start();
        }

        private void shutdownMethod()
        {

            foreach (object obj in this._threadedObjects)
            {
                try
                {
                    Logger.instance().addToLog("Attempting to shut down threaded system: " + obj.GetType(),
                                               Logger.LogTypes.General);
                    ((IThreadedSystem) obj).stop();
                }
                catch (NotImplementedException ex)
                {
                    GlobalFunctions.errorLog(ex);
                }
            }

            Logger.instance().addToLog("All threaded systems have been shut down.", Logger.LogTypes.General);
        }

        /// <summary>
        /// Gets all thread status.
        /// </summary>
        /// <returns></returns>
        public string[] getAllThreadStatus()
        {
            ArrayList responses = new ArrayList();
            foreach (IThreadedSystem item in this._threadedObjects)
            {
                string status = item.GetType() + ": ";
                try
                {
                    foreach (string i in item.getThreadStatus())
                    {
                        responses.Add(status + i);
                    }
                }
                catch (NotImplementedException)
                {
                    status += "Not available.";
                    responses.Add(status);
                }
            }

            string[] responseArray = new string[responses.Count];

            responses.CopyTo(responseArray);

            return responseArray;
        }
    }
}