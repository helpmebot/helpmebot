// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Status.cs" company="Helpmebot Development Team">
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
//   Category watcher status subcomand
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    using Helpmebot;
    using Helpmebot.Monitoring;

    using helpmebot6.Monitoring;

    /// <summary>
    /// Category watcher status subcommand
    /// </summary>
    internal class Status : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Status"/> class.
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
        public Status(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] messageParams =
                {
                    this.Arguments[0],
                    WatcherController.instance().isWatcherInChannel(this.Channel, this.Arguments[0])
                        ? new Message().get("enabled")
                        : new Message().get("disabled"),
                    WatcherController.instance().getDelay(this.Arguments[0]).ToString()
                };

            return new CommandResponseHandler(new Message().get("keywordStatus", messageParams));
        }
    }
}