// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ping.cs" company="Helpmebot Development Team">
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
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///     The ping command
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Ping : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Ping"/> class.
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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Ping(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
        /// </summary>
        /// <returns>Command response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string name;
            string message;

            IMessageService messageService = this.CommandServiceHelper.MessageService;
            if (this.Arguments.Length == 0)
            {
                name = this.Source.Nickname;
                string[] messageparams = { name };
                message = messageService.RetrieveMessage("cmdPing", this.Channel, messageparams);
            }
            else
            {
                name = string.Join(" ", this.Arguments);
                string[] messageparams = { name };
                message = messageService.RetrieveMessage("cmdPingUser", this.Channel, messageparams);
            }

            return new CommandResponseHandler(message);
        }

        #endregion
    }
}