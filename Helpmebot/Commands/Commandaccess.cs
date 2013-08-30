﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Commandaccess.cs" company="Helpmebot Development Team">
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
//   Returns the accesslevel of the command specified
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;

    /// <summary>
    /// Returns the accesslevel of the command specified
    /// </summary>
    internal class Commandaccess : GenericCommand
    {
        public Commandaccess(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {

            if (args.Length > 0)
            {
                // find the command
                Type cmd = Type.GetType(
                    "helpmebot6.Commands." + args[0].Substring(0, 1).ToUpper() + args[0].Substring(1).ToLower()
                    );
                // check it exists
                if (cmd == null) //TODO: return an error message instead
                    return null;
                return // instantiate a new instance of the command, and get it's access level
                    new CommandResponseHandler(
                        ((GenericCommand)Activator.CreateInstance(cmd, source, channel, args)).AccessLevel.ToString());
            }
            else
            {
                string[] messageParameters = {"commandaccess", "1", args.Length.ToString()};
                return new CommandResponseHandler(new Message().get("notEnoughParameters", messageParameters));

            }
        }
    }
}