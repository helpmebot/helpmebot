// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Die.cs" company="Helpmebot Development Team">
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
//   Kills the bot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///   Kills the bot.
    /// </summary>
    internal class Die : ProtectedCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Die"/> class.
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
        public Die(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>null - the bot should be shutting down</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            Helpmebot6.Stop();
            return null;
        }

        /// <summary>
        /// The not confirmed.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler NotConfirmed()
        {
            return new CommandResponseHandler(this.MessageService.RetrieveMessage("Die-unconfirmed", this.Channel, null));
        }
    }
}