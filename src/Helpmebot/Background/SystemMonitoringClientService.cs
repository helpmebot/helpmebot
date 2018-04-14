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
    using Helpmebot.Configuration;

    /// <summary>
    ///     The system monitoring client service.
    /// </summary>
    public class SystemMonitoringClientService : ISystemMonitoringClientService
    {
        private readonly bool enabled;
        private readonly string message;
        private readonly int port;

        private readonly ILogger logger;
        private readonly IIrcClient networkClient;
        private readonly Thread monitorthread;

        private bool alive;

        public SystemMonitoringClientService(BotConfiguration configuration,
            ILogger logger,
            IIrcClient networkClient)
        {
            this.enabled = configuration.SystemMonitoringPort.HasValue;
            this.logger = logger;
            this.networkClient = networkClient;
            this.port = configuration.SystemMonitoringPort.GetValueOrDefault();
            this.message = "Helpmebot v6 (Nagios Monitor service)";

            if (!this.enabled)
            {
                this.logger.WarnFormat("{0} is disabled and will not function.", this.GetType().Name);
                return;
            }

            this.monitorthread = new Thread(this.ThreadMethod);

            this.logger.Info("Initialised Monitoring Client.");
        }
        
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
        /// Stop all threads in this instance to allow for a clean shutdown.
        /// </summary>
        public void Stop()
        {
            this.logger.Info("Stopping Monitoring Client.");
            this.alive = false;
        }

        private void ThreadMethod()
        {
            this.alive = true;

            var service = new TcpListener(IPAddress.Any, this.port);
            service.Start();
            this.logger.Debug("Started Monitoring Client.");
            while (this.alive)
            {
                if (!service.Pending())
                {
                    Thread.Sleep(10);
                    continue;
                }

                this.logger.Debug("Found waiting request.");

                TcpClient client = service.AcceptTcpClient();

                var sw = new StreamWriter(client.GetStream());

                if (!this.networkClient.NetworkConnected)
                {
                    sw.WriteLine("IRC client is not connected to the network!");
                }
                else
                {
                    sw.WriteLine(this.message);
                }

                sw.Flush();
                client.Close();
            }

            service.Stop();
            this.logger.Info("Stopped Monitoring Client.");
        }
    }
}