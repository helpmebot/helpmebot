// --------------------------------------------------------------------------------------------------------------------
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

namespace Helpmebot.Threading
{
    using System;
    using System.Collections;
    using System.Threading;

    using Castle.Core.Logging;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Maintains a list of all the available threads the bot is running
    /// </summary>
    internal class ThreadList
    {
        /// <summary>
        /// The _instance.
        /// </summary>
        private static ThreadList singletonInstance;

        /// <summary>
        /// The log.
        /// </summary>
        private readonly ILogger log;

        /// <summary>
        /// Initialises a new instance of the <see cref="ThreadList"/> class.
        /// </summary>
        protected ThreadList()
        {
            this.ThreadedObjects = new ArrayList();

            // FIXME: ServiceLocator - logger
            this.log = ServiceLocator.Current.GetInstance<ILogger>();
        }

        /// <summary>
        /// Gets the threaded objects.
        /// </summary>
        public ArrayList ThreadedObjects { get; private set; }

        /// <summary>
        /// The instance.
        /// </summary>
        /// <returns>
        /// The <see cref="ThreadList"/>.
        /// </returns>
        public static ThreadList GetInstance()
        {
            return singletonInstance ?? (singletonInstance = new ThreadList());
        }
        
        /// <summary>
        /// Registers the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public void Register(IThreadedSystem sender)
        {
            this.ThreadedObjects.Add(sender);
        }

        /// <summary>
        /// Creates a new thread to start shutting down other threads
        /// </summary>
        public void Stop()
        {
            var shutdownControllerThread = new Thread(this.ShutdownMethod);

            shutdownControllerThread.Start();
        }

        /// <summary>
        /// The get all thread status.
        /// </summary>
        /// <returns>
        /// The array of strings.
        /// </returns>
        public string[] GetAllThreadStatus()
        {
            var responses = new ArrayList();
            foreach (IThreadedSystem item in this.ThreadedObjects)
            {
                var status = item.GetType() + ": ";
                try
                {
                    foreach (string i in item.GetThreadStatus())
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

            var responseArray = new string[responses.Count];

            responses.CopyTo(responseArray);

            return responseArray;
        }

        /// <summary>
        /// The shutdown method.
        /// </summary>
        private void ShutdownMethod()
        {
            foreach (var obj in this.ThreadedObjects)
            {
                try
                {
                    this.log.Info("Attempting to shut down threaded system: " + obj.GetType());
                    ((IThreadedSystem)obj).Stop();
                }
                catch (NotImplementedException ex)
                {
                    this.log.Error(ex.Message, ex);
                }
            }

            this.log.Info("All threaded systems have been shut down.");
        }
    }
}