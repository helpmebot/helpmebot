﻿// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    /// Retrieves the bot access lvel of the user who called the command
    /// </summary>
    internal class Myaccess : GenericCommand
    {
        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            var crh = new CommandResponseHandler();

            if (args.Length > 0 && args[0] != string.Empty)
            {
                foreach (string s in args)
                {
                    string[] cmdArgs = {s, User.newFromString(s).accessLevel.ToString()};
                    crh.respond(new Message().get("cmdAccess", cmdArgs));
                }
            }

            else
            {
                string[] cmdArgs = {source.ToString(), source.accessLevel.ToString()};
                crh.respond(new Message().get("cmdAccess", cmdArgs));
            }
            return crh;
        }
    }
}