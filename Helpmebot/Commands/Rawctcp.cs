// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rawctcp.cs" company="Helpmebot Development Team">
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
//   Sends a raw client-to-client protocol command
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    /// <summary>
    /// Sends a raw client-to-client protocol command
    /// </summary>
    internal class Rawctcp : GenericCommand
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
            string cmd = GlobalFunctions.popFromFront(ref args);
            string dst = GlobalFunctions.popFromFront(ref args);

            Helpmebot6.irc.ircPrivmsg(dst, IAL.wrapCTCP(cmd, string.Join(" ", args)));

            return null;
        }
    }
}