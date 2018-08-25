// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProtectedCommand.cs" company="Helpmebot Development Team">
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
    using System;
    using System.Linq;

    using Helpmebot;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    ///     The protected command.
    /// </summary>
    public abstract class ProtectedCommand : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="ProtectedCommand"/> class.
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
        protected ProtectedCommand(
            IUser source, 
            string channel, 
            string[] args, 
            ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The not confirmed.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected abstract CommandResponseHandler NotConfirmed();

        /// <summary>
        ///     The really run command.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected override CommandResponseHandler ReallyRunCommand()
        {
            if (
                !AccessLog.Instance()
                     .Save(
                         new AccessLog.AccessLogEntry(
                     this.Source, 
                     this.GetType(), 
                     true, 
                     this.Channel, 
                     this.Arguments, 
                     this.AccessLevel)))
            {
                var errorResponse = new CommandResponseHandler();
                errorResponse.Respond(
                    "Error adding to access log - command aborted.", 
                    CommandResponseDestination.ChannelDebug);
                string message =
                    this.CommandServiceHelper.MessageService.RetrieveMessage(
                        "AccessDeniedAccessListFailure", 
                        this.Channel, 
                        null);
                errorResponse.Respond(message, CommandResponseDestination.Default);
                return errorResponse;
            }

            this.Log.Info("Starting command execution...");
            CommandResponseHandler crh;

            try
            {
                crh = this.Arguments.Contains("@confirm") ? this.ExecuteCommand() : this.NotConfirmed();
            }
            catch (Exception ex)
            {
                this.Log.Error(ex.Message, ex);
                crh = new CommandResponseHandler(ex.Message);
            }

            this.Log.Info("Command execution complete.");
            return crh;
        }

        #endregion
    }
}