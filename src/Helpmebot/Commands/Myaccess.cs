// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Myaccess.cs" company="Helpmebot Development Team">
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
//   Retrieves the bot access lvel of the user who called the command
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot;

    /// <summary>
    /// Retrieves the bot access level of the user who called the command
    /// </summary>
    internal class Myaccess : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Myaccess"/> class.
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
        public Myaccess(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var crh = new CommandResponseHandler();

            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                foreach (string s in this.Arguments)
                {
                    string[] cmdArgs = { s, User.newFromString(s).accessLevel.ToString() };
                    crh.respond(new Message().get("cmdAccess", cmdArgs));
                }
            }
            else
            {
                string[] cmdArgs = { this.Source.ToString(), this.Source.accessLevel.ToString() };
                crh.respond(new Message().get("cmdAccess", cmdArgs));
            }

            return crh;
        }
    }
}