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
    using Castle.Core.Logging;
    using Stwalkerster.IrcClient.Interfaces;
    using Helpmebot.Background.Interfaces;

    /// <summary>
    ///     The system monitoring client service.
    /// </summary>
    public class SystemMonitoringClientService : ISystemMonitoringClientService
    {
        #region Fields

        private readonly bool enabled;

        /// <summary>
        ///     The logger.
        /// </summary>
        private readonly ILogger logger;

        private readonly IIrcClient networkClient;

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
        /// <param name="systemMonitoringPort">
        /// The system Monitoring Port.
        /// </param>
        /// <param name="systemMonitoringMessage">
        /// The system Monitoring Message.
        /// </param>
        /// <param name="systemMonitoringEnabled">
        /// The system Monitoring Enabled.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public SystemMonitoringClientService(int systemMonitoringPort, string systemMonitoringMessage,
            bool systemMonitoringEnabled, ILogger logger, IIrcClient networkClient)
        {
            this.enabled = systemMonitoringEnabled;
            this.logger = logger;
            this.networkClient = networkClient;
            this.Port = systemMonitoringPort;
            this.Message = systemMonitoringMessage;

            if (!this.enabled)
            {
                this.logger.WarnFormat("{0} is disabled and will not function.", this.GetType().Name);
                return;
            }

            this.monitorthread = new Thread(this.ThreadMethod);

            this.service = new TcpListener(IPAddress.Any, this.Port);

            this.logger.Info("Initialised Monitoring Client.");
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
            if (!this.enabled)
            {
                this.logger.WarnFormat("{0} is disabled and will not function.", this.GetType().Name);
                return;
            }

            this.logger.Info("Starting Monitoring Client...");
            this.monitorthread.Start();
        }

        /// <summary>
        ///     Stop all threads in this instance to allow for a clean shutdown.
        /// </summary>
        public void Stop()
        {
            this.logger.Info("Stopping Monitoring Client.");
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
            this.logger.Debug("Started Monitoring Client.");
            while (this.alive)
            {
                if (!this.service.Pending())
                {
                    Thread.Sleep(10);
                    continue;
                }

                this.logger.Debug("Found waiting request.");

                TcpClient client = this.service.AcceptTcpClient();

                var sw = new StreamWriter(client.GetStream());

                if (!this.networkClient.NetworkConnected)
                {
                    sw.WriteLine("IRC client is not connected to the network!");
                }
                else
                {
                    sw.WriteLine(this.Message);                    
                }
                
                sw.Flush();
                client.Close();
            }

            this.service.Stop();
            this.logger.Info("Stopped Monitoring Client.");
        }

        #endregion
    }
}