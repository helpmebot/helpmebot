﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThreadStatus.cs" company="Helpmebot Development Team">
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
//   Retrieve the internal status of the bot's subsystems
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Threading;

    /// <summary>
    /// Retrieve the internal status of the bot's subsystems
    /// </summary>
    internal class Threadstatus : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Threadstatus"/> class.
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
        public Threadstatus(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] statuses = ThreadList.GetInstance().GetAllThreadStatus();
            CommandResponseHandler crh = new CommandResponseHandler();
            foreach (string item in statuses)
            {
                crh.Respond(item);
            }

            return crh;
        }
    }
}