// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Raw.cs" company="Helpmebot Development Team">
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

using Stwalkerster.IrcClient;

namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    ///     Send a raw line to IRC
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Developer)]
    internal class Raw : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Raw"/> class.
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
        public Raw(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var ircClient = this.CommandServiceHelper.Client as IrcClient;
            if (ircClient != null)
            {
                ircClient.Inject(this.Arguments.Implode());
            }
            else
            {
                this.CommandServiceHelper.Client.SendMessage(this.Source.Nickname, "Error injecting message into network stream.");
            }

            return new CommandResponseHandler();
        }

        #endregion
    }
}