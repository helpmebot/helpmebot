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
// <summary>
//   Defines the ProtectedCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;

    /// <summary>
    /// The protected command.
    /// </summary>
    public abstract class ProtectedCommand : GenericCommand
    {
        protected ProtectedCommand(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// The really run command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ReallyRunCommand()
        {
            if (
                !AccessLog.instance()
                     .save(new AccessLog.AccessLogEntry(this.Source, GetType(), true, this.Channel, this.Arguments)))
            {
                CommandResponseHandler errorResponse = new CommandResponseHandler();
                errorResponse.respond("Error adding to access log - command aborted.", CommandResponseDestination.ChannelDebug);
                errorResponse.respond(new Message().get("AccessDeniedAccessListFailure"), CommandResponseDestination.Default);
                return errorResponse;
            }

            this.LogMessage("Starting command execution...");
            CommandResponseHandler crh;

            try
            {
                crh = GlobalFunctions.isInArray("@confirm", this.Arguments) != -1 ? this.ExecuteCommand() : this.NotConfirmed(this.Source, this.Channel, this.Arguments);
            }
            catch (Exception ex)
            {
                Logger.instance().addToLog(ex.ToString(), Logger.LogTypes.Error);
                crh = new CommandResponseHandler(ex.Message);
            }

            this.LogMessage("Command execution complete.");
            return crh;
        }

        /// <summary>
        /// The not confirmed.
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
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected abstract CommandResponseHandler NotConfirmed(User source, string channel, string[] args);
    }
}
