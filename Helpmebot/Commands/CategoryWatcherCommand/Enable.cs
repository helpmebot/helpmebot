// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Enable.cs" company="Helpmebot Development Team">
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
//   Category watcher enable subcommand
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands.CategoryWatcherCommand
{
    using helpmebot6.Monitoring;

    /// <summary>
    /// Category watcher enable subcommand 
    /// </summary>
    internal class Enable : GenericCommand
    {
        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            return WatcherController.instance().addWatcherToChannel(this.Arguments[0], this.Channel)
                       ? new CommandResponseHandler(new Message().get("done"))
                       : new CommandResponseHandler(new Message().get("no-change"));
        }
    }
}