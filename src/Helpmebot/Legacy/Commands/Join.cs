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
    using System.Globalization;

    using Helpmebot;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Join(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
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
            LegacyDatabase.Select q = new LegacyDatabase.Select("count(*)");
            q.AddWhere(new LegacyDatabase.WhereConds("channel_name", args));
            q.AddWhere(new LegacyDatabase.WhereConds("channel_network", network.ToString(CultureInfo.InvariantCulture)));
            q.SetFrom("channel");

            string count = LegacyDatabase.Singleton().ExecuteScalarSelect(q);

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
                LegacyDatabase.Singleton().Update("channel", vals, 1, new LegacyDatabase.WhereConds("channel_name", args));

                Helpmebot6.irc.IrcJoin(args);
            }
            else
            {
                LegacyDatabase.Singleton().Insert("channel", string.Empty, args, string.Empty, "1", network.ToString(CultureInfo.InvariantCulture));
                Helpmebot6.irc.IrcJoin(args);
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
                return JoinChannel(this.Arguments[0], this.Source.Network);
            }

            string[] messageParameters = { "join", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture) };
            return new CommandResponseHandler(this.MessageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
        }

        /// <summary>
        /// The test access.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected override bool TestAccess()
        {
            if (!base.TestAccess())
            {
                return false;
            }

            if (this.Arguments.Length >= 1)
            {
                if (this.Arguments[0] == "0")
                {
                    return false;
                }
            }

            return true;
        }
    }
}