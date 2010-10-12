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

using System.Collections.Generic;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Joins an IRC channel
    /// </summary>
    internal class Join : GenericCommand
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
            return joinChannel(args[0], source.network);
        }

        public static CommandResponseHandler joinChannel(string args, uint network)
        {
            DAL.Select q = new DAL.Select("count(*)");
            q.addWhere(new DAL.WhereConds("channel_name", args));
            q.addWhere(new DAL.WhereConds("channel_network", network.ToString()));
            q.setFrom("channel");

            string count = DAL.singleton().executeScalarSelect(q);


            if (count == "1")
            {
                // entry exists

                Dictionary<string, string> vals = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "channel_enabled",
                                                              "1"
                                                              }
                                                      };
                DAL.singleton().update("channel", vals, 1, new DAL.WhereConds("channel_name", args));

                Helpmebot6.irc.ircJoin(args);
            }
            else
            {
                DAL.singleton().insert("channel", "", args, "", "1", network.ToString());
                Helpmebot6.irc.ircJoin(args);
            }
            return null;
        }
    }
}