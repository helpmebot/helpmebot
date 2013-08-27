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
    using helpmebot6.Monitoring;

    /// <summary>
    /// Category watcher delay subcommand
    /// </summary>
    internal class Delay : GenericCommand
    {
        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            if (args.Length > 2)
            {
                // 2 or more args
                return WatcherController.instance().setDelay(args[0], int.Parse(args[2]));
            }
            if (args.Length == 2)
            {
                int delay = WatcherController.instance().getDelay(args[0]);
                string[] messageParams = {args[0], delay.ToString()};
                string message = new Message().get("catWatcherCurrentDelay", messageParams);
                return new CommandResponseHandler(message);
            }
            // TODO: fix
            return null;
        }
    }
}