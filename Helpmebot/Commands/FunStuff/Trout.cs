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
    /// Trouts a user
    /// </summary>
    internal class Trout : FunStuff.FunCommand
    {
        protected readonly string[] forbiddenTargets = {
                                            "itself", "himself", "herself", "themself", 
                                            Helpmebot6.irc.ircNickname.ToLower()
                                        };

        protected string messageName = "CmdTrout";

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string name = string.Join(" ", args);

            if (GlobalFunctions.isInArray(name.ToLower(), forbiddenTargets) != -1)
            {
                name = source.nickname;
            }

            string[] messageparams = {name};
            string message = IAL.wrapCTCP("ACTION", new Message().get(messageName, messageparams));

            return new CommandResponseHandler(message);
        }
    }
}