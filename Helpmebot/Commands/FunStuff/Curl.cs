// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Curl.cs" company="Helpmebot Development Team">
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
//   Defines the Curl type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    /// <summary>
    /// The curl.
    /// </summary>
    class Curl : FunStuff.FunCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Curl"/> class.
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
        public Curl(User source, string channel, string[] args)
            : base(source, channel, args)
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
            Configuration.singleton()["hedgehog", this.Channel] = "true";
            return new CommandResponseHandler(new Message().get("HedgehogCurlup"));
        }
    }
}
