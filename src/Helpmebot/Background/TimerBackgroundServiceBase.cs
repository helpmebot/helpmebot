// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerBackgroundServiceBase.cs" company="Helpmebot Development Team">
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
//   Defines the TimerBackgroundServiceBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Background
{
    using System.Timers;

    using Castle.Core.Logging;

    using Helpmebot.Background.Interfaces;

    /// <summary>
    /// The timer background service base.
    /// </summary>
    public abstract class TimerBackgroundServiceBase : ITimerBackgroundService
    {
        /// <summary>
        /// The interval.
        /// </summary>
        private int interval;

        /// <summary>
        /// Initialises a new instance of the <see cref="TimerBackgroundServiceBase"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="interval">
        /// The interval.
        /// </param>
        protected TimerBackgroundServiceBase(ILogger logger, int interval)
        {
            this.Logger = logger;
            this.Logger.DebugFormat("Creating instance of {0} as TimerBackgroundService", this.GetType().Name);
            this.Interval = interval;
            this.Timer = new Timer(this.Interval);
            this.Logger.DebugFormat("Done creating instance of {0} as TimerBackgroundService", this.GetType().Name);
        }

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        public int Interval
        {
            get
            {
                return this.interval;
            }

            set
            {
                this.Logger.DebugFormat("Setting interval to {0}", value);
                this.interval = value;

                if (this.Timer != null)
                {
                    this.Timer.Interval = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the timer.
        /// </summary>
        protected Timer Timer { get; set; }

        /// <summary>
        /// The start.
        /// </summary>
        public void Start()
        {
            this.Timer.Elapsed += this.TimerOnElapsedBase;
            this.Timer.Enabled = true;
            this.OnStart();
            this.Logger.InfoFormat("Starting instance of {0}", this.GetType().Name);
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public void Stop()
        {
            this.Logger.InfoFormat("Stopping instance of {0}", this.GetType().Name);
            this.Timer.Enabled = false;
            this.Timer.Elapsed -= this.TimerOnElapsedBase;
            this.OnStop();
        }

        /// <summary>
        /// The on start.
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// The on stop.
        /// </summary>
        protected virtual void OnStop()
        {
        }

        /// <summary>
        /// The timer on elapsed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="elapsedEventArgs">
        /// The elapsed event args.
        /// </param>
        protected abstract void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs);

        /// <summary>
        /// The timer on elapsed base.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TimerOnElapsedBase(object sender, ElapsedEventArgs e)
        {
            this.Logger.Debug("Event raised!");
            this.TimerOnElapsed(sender, e);
        }
    }
}