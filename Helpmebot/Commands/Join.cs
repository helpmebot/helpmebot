// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Join.cs" company="Helpmebot Development Team">
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
//   Joins an IRC channel
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Collections.Generic;

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
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            if(args.Length >= 1)
            {
                if (args[0] == "0") return this.OnAccessDenied(source, channel, args);

                return joinChannel(args[0], source.network);
            }
            else
            {
                string[] messageParameters = { "join", "1", args.Length.ToString() };
                return new CommandResponseHandler(new Message().get("notEnoughParameters", messageParameters));

            }
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