// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericCommand.cs" company="Helpmebot Development Team">
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
//   Generic bot command abstract class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Generic bot command abstract class
    /// </summary>
    public abstract class GenericCommand
    {
        /// <summary>
        /// Gets the access level of the command
        /// </summary>
        /// <value>The access level.</value>
        public User.UserRights AccessLevel
        {
            get
            {
                string command = GetType().ToString();

                DAL.Select q = new DAL.Select("accesslevel");
                q.setFrom("command");
                q.addLimit(1, 0);
                q.addWhere(new DAL.WhereConds("typename", command));

                string al = DAL.singleton().executeScalarSelect(q);
                try
                {
                    return (User.UserRights)Enum.Parse(typeof(User.UserRights), al, true);
                }
                catch (ArgumentException)
                {
                    Logger.instance()
                        .addToLog("Warning: " + command + " not found in access list.", Logger.LogTypes.Error);
                    return User.UserRights.Developer;
                }
            }
        }

        /// <summary>
        /// Trigger an execution of the command
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">Arguments to the command.</param>
        /// <returns>the response container</returns>
        public CommandResponseHandler RunCommand(User source, string channel, string[] args)
        {
            string command = GetType().ToString();

            this.LogMessage("Running command: " + command);

            return this.TestAccess(source, channel)
                       ? this.ReallyRunCommand(source, channel, args)
                       : this.OnAccessDenied(source, channel, args);
        }

        /// <summary>
        /// Check the access level and then decide what to do.
        /// </summary>
        /// <param name="source">The source of the command</param>
        /// <param name="channel">The channel the command was triggered in</param>
        /// <returns>True if the command is allowed to Execute</returns>
        protected virtual bool TestAccess(User source, string channel)
        {
            // check the access level
            return source.accessLevel >= this.AccessLevel;
        }

        /// <summary>
        /// Access granted to command, decide what to do
        /// </summary>
        /// <param name="source">The source of the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">Arguments to the command</param>
        /// <returns>The response to the command</returns>
        protected virtual CommandResponseHandler ReallyRunCommand(User source, string channel, string[] args)
        {
            if (!AccessLog.instance().save(new AccessLog.AccessLogEntry(source, GetType(), true, channel, args)))
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
                crh = this.ExecuteCommand(source, channel, args);
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
        /// Access denied to command, decide what to do
        /// </summary>
        /// <param name="source">The source of the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns>A response to the command if access to the command was denied</returns>
        protected virtual CommandResponseHandler OnAccessDenied(User source, string channel, string[] args)
        {
            CommandResponseHandler response = new CommandResponseHandler();

            response.respond(new Message().get("OnAccessDenied", string.Empty), CommandResponseDestination.PrivateMessage);
            this.LogMessage("Access denied to command.");
            if (!AccessLog.instance().save(new AccessLog.AccessLogEntry(source, GetType(), false, channel, args)))
            {
                response.respond("Error adding denied entry to access log.", CommandResponseDestination.ChannelDebug);
            }

            return response;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns>The response to the command</returns>
        protected abstract CommandResponseHandler ExecuteCommand(User source, string channel, string[] args);

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void LogMessage(string message)
        {
            Logger.instance()
                .addToLog(MethodBase.GetCurrentMethod().DeclaringType.Name + ": " + message, Logger.LogTypes.Command);
        }
    }
}