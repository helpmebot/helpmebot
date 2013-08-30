// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SayHi.cs" company="Helpmebot Development Team">
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
//   Says hi to a user
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Says hi to a user
    /// </summary>
    internal class Sayhi : FunStuff.FunCommand
    {
        /// <summary>
        /// Executes the specified to user.
        /// </summary>
        /// <param name="toUser">To user.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User toUser, string channel, string[] args)
        {
            string[] commandParams = {toUser.nickname};
            return new CommandResponseHandler(new Message().get("cmdSayHi1", commandParams));
        }
    }
}