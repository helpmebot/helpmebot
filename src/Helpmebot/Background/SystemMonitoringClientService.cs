// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemMonitoringClientService.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Background
{
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    using Helpmebot.Background.Interfaces;

    /// <summary>
    ///     The system monitoring client service.
    /// </summary>
    public class SystemMonitoringClientService : ISystemMonitoringClientService
    {
        #region Fields

        /// <summary>
        ///     The _monitor thread.
        /// </summary>
        private readonly Thread monitorthread;

        /// <summary>
        ///     The _service.
        /// </summary>
        private readonly TcpListener service;

        /// <summary>
        ///     The _alive.
        /// </summary>
        private bool alive;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="SystemMonitoringClientService"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        public SystemMonitoringClientService(string message, int port)
        {
            this.Port = port;
            this.Message = message;
            
            this.monitorthread = new Thread(this.ThreadMethod);

            this.service = new TcpListener(IPAddress.Any, this.Port);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        ///     Gets the port.
        /// </summary>
        public int Port { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The start.
        /// </summary>
        public void Start()
        {
            this.monitorthread.Start();
        }

        /// <summary>
        ///     Stop all threads in this instance to allow for a clean shutdown.
        /// </summary>
        public void Stop()
        {
            this.alive = false;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The thread method.
        /// </summary>
        private void ThreadMethod()
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

                sw.WriteLine(this.Message);
                sw.Flush();
                client.Close();
            }

            this.service.Stop();
        }

        #endregion
    }
}