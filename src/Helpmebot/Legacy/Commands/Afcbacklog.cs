// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Afccount.cs" company="Helpmebot Development Team">
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
//   Returns the number of articles currently waiting at Articles for Creation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;

    /// <summary>
    /// Returns the number of articles currently waiting at Articles for Creation
    /// </summary>
    internal class Afccount : Categorysize
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Afccount"/> class.
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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Afccount(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            return this.GetSizeOfCategory("Pending AfC submissions");
            if (CommandResponseHandler(message) = 0) {
                "There are no new AfC submissions."
            }
            else if (CommandResponseHandler(message) < 200) {
                "AfC is clearing out."
            }
            else if (CommandResponseHandler(message) < 400) {
                "There is a normal backlog at AfC."
            }
            else if (CommandResponseHandler(message) < 650) {
                "AfC is semi-backlogged at the moment."
            }
            else if (CommandResponseHandler(message) < 900) {
                "There is a backlog at AfC."
            }
            else if (CommandResponseHandler(message) < 1200) {
                "AfC is highly backlogged at the moment."
            }
            else if (CommandResponseHandler(message) < 2000) {
                "There is a severe backlog at AfC."
            }
            else if (CommandResponseHandler(message) < 4000) {
                "AfC is critically backlogged."
            }
            else if (CommandResponseHandler(message) < 10000) {
                "AfC is out of order."
            }
            else {
                "I am clueless to the backlog at AfC. Please contact my owner, stwalkster."
            }
        }
    }
}
