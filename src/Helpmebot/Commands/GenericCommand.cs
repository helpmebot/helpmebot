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

    using Castle.Core.Logging;

    using Helpmebot;
    using Helpmebot.Legacy.Database;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Generic bot command abstract class
    /// </summary>
    public abstract class GenericCommand
    {
        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="GenericCommand"/> class.
        /// </summary>
        public GenericCommand()
        {
            // FIXME: Remove me!
            this.Log = ServiceLocator.Current.GetInstance<ILogger>();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="GenericCommand"/> class.
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
        public GenericCommand(User source, string channel, string[] args)
        {
            // FIXME: Remove me!
            this.Log = ServiceLocator.Current.GetInstance<ILogger>();

            this.Source = source;
            this.Channel = channel;
            this.Arguments = args;
        }

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
                    Log.Warn("Warning: " + command + " not found in access list.");
                    return User.UserRights.Developer;
                }
            }
        }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public User Source { get; set; }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        public string[] Arguments { get; set; }

        /// <summary>
        /// Trigger an execution of the command
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">Arguments to the command.</param>
        /// <returns>the response container</returns>
        [Obsolete]
        public CommandResponseHandler RunCommand(User source, string channel, string[] args)
        {
            this.Source = source;
            this.Channel = channel;
            this.Arguments = args;

            return this.RunCommand();
        }

        /// <summary>
        /// The run command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        public CommandResponseHandler RunCommand()
        {
            string command = GetType().ToString();

            Log.Info("Running command: " + command);

            return this.TestAccess()
                       ? this.ReallyRunCommand()
                       : this.OnAccessDenied();
        }

        /// <summary>
        /// Check the access level and then decide what to do.
        /// </summary>
        /// <returns>True if the command is allowed to Execute</returns>
        protected virtual bool TestAccess()
        {
            // check the access level
            return this.Source.accessLevel >= this.AccessLevel;
        }

        /// <summary>
        /// Access granted to command, decide what to do
        /// </summary>
        /// <returns>The response to the command</returns>
        protected virtual CommandResponseHandler ReallyRunCommand()
        {
            if (!AccessLog.instance().save(new AccessLog.AccessLogEntry(this.Source, GetType(), true, this.Channel, this.Arguments)))
            {
                CommandResponseHandler errorResponse = new CommandResponseHandler();
                errorResponse.respond("Error adding to access log - command aborted.", CommandResponseDestination.ChannelDebug);
                errorResponse.respond(new Message().get("AccessDeniedAccessListFailure"), CommandResponseDestination.Default);
                return errorResponse;
            }

            Log.Info("Starting command execution...");
            CommandResponseHandler crh;
            try
            {
                crh = this.ExecuteCommand();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                crh = new CommandResponseHandler(ex.Message);
            }

            Log.Info("Command execution complete.");
            return crh;
        }

        /// <summary>
        /// Access denied to command, decide what to do
        /// </summary>
        /// <returns>A response to the command if access to the command was denied</returns>
        protected virtual CommandResponseHandler OnAccessDenied()
        {
            CommandResponseHandler response = new CommandResponseHandler();

            response.respond(new Message().get("OnAccessDenied", string.Empty), CommandResponseDestination.PrivateMessage);
            Log.Info("Access denied to command.");
            if (!AccessLog.instance().save(new AccessLog.AccessLogEntry(this.Source, GetType(), false, this.Channel, this.Arguments)))
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
        [Obsolete]
        protected virtual CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            return new CommandResponseHandler("not implemented");
        }

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected virtual CommandResponseHandler ExecuteCommand()
        {
#pragma warning disable 612
            return this.ExecuteCommand(this.Source, this.Channel, this.Arguments);
#pragma warning restore 612
        }
    }
}