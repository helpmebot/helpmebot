// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Part.cs" company="Helpmebot Development Team">
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
//   Leave an IRC channel
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Collections.Generic;

    /// <summary>
    /// Leave an IRC channel
    /// </summary>
    internal class Part : GenericCommand
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
            Helpmebot6.irc.ircPart(channel, source.ToString());
            Dictionary<string, string> vals = new Dictionary<string, string>
                                                  { { "channel_enabled", "0" } };
            DAL.singleton().update("channel", vals, 0, new DAL.WhereConds("channel_name", channel),
                                   new DAL.WhereConds("channel_network", source.network.ToString()));
            return null;
        }
    }
}