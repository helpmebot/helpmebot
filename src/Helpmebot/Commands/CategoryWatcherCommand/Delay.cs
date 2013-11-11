// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Delay.cs" company="Helpmebot Development Team">
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
//   Category watcher delay subcommand
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    using Helpmebot;
    using Helpmebot.Monitoring;

    /// <summary>
    /// Category watcher delay subcommand
    /// </summary>
    internal class Delay : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Delay"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public Delay(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length > 2)
            {
                // 2 or more args
                return WatcherController.Instance().SetDelay(this.Arguments[0], int.Parse(this.Arguments[2]));
            }

            if (this.Arguments.Length == 2)
            {
                int delay = WatcherController.Instance().GetDelay(this.Arguments[0]);
                string[] messageParams = { this.Arguments[0], delay.ToString() };
                string message = new Message().GetMessage("catWatcherCurrentDelay", messageParams);
                return new CommandResponseHandler(message);
            }

            // TODO: fix
            return null;
        }
    }
}