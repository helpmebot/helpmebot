// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Isgd.cs" company="Helpmebot Development Team">
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
//   Shortens a URL
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;

    using Helpmebot;

    /// <summary>
    /// Shortens a URL
    /// </summary>
    internal class Isgd : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Isgd"/> class.
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
        public Isgd(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The result</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length == 0)
            {
                string[] messageParameters = { "isgd", "1", this.Arguments.Length.ToString() };
                return new CommandResponseHandler(new Message().GetMessage("notEnoughParameters", messageParameters));
            }

            return new CommandResponseHandler(IsGd.shorten(new Uri(this.Arguments[0])).ToString());
        }
    }
}