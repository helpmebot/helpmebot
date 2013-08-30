﻿// --------------------------------------------------------------------------------------------------------------------
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
        /// Initialises a new instance of the <see cref="Join"/> class.
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
        public Join(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// The join channel.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="network">
        /// The network.
        /// </param>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        /// TODO: this should probably be elsewhere
        public static CommandResponseHandler JoinChannel(string args, uint network)
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
                DAL.singleton().insert("channel", string.Empty, args, string.Empty, "1", network.ToString());
                Helpmebot6.irc.ircJoin(args);
            }

            return null;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length >= 1)
            {
                if (this.Arguments[0] == "0")
                {
                    return this.OnAccessDenied(); // TODO: put this in the access check, not the execution
                }

                return JoinChannel(this.Arguments[0], this.Source.network);
            }

            string[] messageParameters = { "join", "1", this.Arguments.Length.ToString() };
            return new CommandResponseHandler(new Message().get("notEnoughParameters", messageParameters));
        }
    }
}