// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonitorService.cs" company="Helpmebot Development Team">
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
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    using Helpmebot.Threading;

    /// <summary>
    /// Monitoring service
    /// </summary>
    internal class MonitorService : IThreadedSystem
    {
        #region Fields

        /// <summary>
        /// The _message.
        /// </summary>
        private readonly string message;

        /// <summary>
        /// The _monitor thread.
        /// </summary>
        private readonly Thread monitorthread;

        /// <summary>
        /// The _service.
        /// </summary>
        private readonly TcpListener service;

        /// <summary>
        /// The _alive.
        /// </summary>
        private bool alive;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="MonitorService"/> class. 
        /// </summary>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public MonitorService(int port, string message)
        {
            this.monitorthread = new Thread(this.ThreadMethod);

            this.message = message;

            this.service = new TcpListener(IPAddress.Any, port);
            this.RegisterInstance();
            this.monitorthread.Start();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The thread fatal error event.
        /// </summary>
        public event EventHandler ThreadFatalErrorEvent;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get thread status.
        /// </summary>
        /// <returns>
        /// The thread status.
        /// </returns>
        public string[] GetThreadStatus()
        {
            string[] status = { "NagiosMonitor thread: " + this.monitorthread.ThreadState };
            return status;
        }

        /// <summary>
        /// The register instance.
        /// </summary>
        public void RegisterInstance()
        {
            ThreadList.GetInstance().Register(this);
        }

        /// <summary>
        ///     Stop all threads in this instance to allow for a clean shutdown.
        /// </summary>
        public void Stop()
        {
            this.service.Stop();
            this.alive = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The thread method.
        /// </summary>
        private void ThreadMethod()
        {
            try
            {
                this.alive = true;
                this.service.Start();

                while (this.alive)
                {
                    if (!this.service.Pending())
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    TcpClient client = this.service.AcceptTcpClient();

                    var sw = new StreamWriter(client.GetStream());

                    sw.WriteLine(this.message);
                    sw.Flush();
                    client.Close();
                }
            }
            catch (ThreadAbortException)
            {
                this.ThreadFatalErrorEvent(this, new EventArgs());
            }
            catch (ObjectDisposedException)
            {
                this.ThreadFatalErrorEvent(this, new EventArgs());
            }
        }

        #endregion
    }
}