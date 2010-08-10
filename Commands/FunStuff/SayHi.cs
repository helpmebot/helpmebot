// /****************************************************************************
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
    ///   Says hi to a user
    /// </summary>
    internal class Sayhi : GenericCommand
    {
        /// <summary>
        /// Executes the specified to user.
        /// </summary>
        /// <param name="toUser">To user.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute(User toUser, string channel, string[] args)
        {
            string[] commandParams = {toUser.nickname};
            return new CommandResponseHandler(Configuration.singleton().getMessage("cmdSayHi1", commandParams));
        }
    }
}