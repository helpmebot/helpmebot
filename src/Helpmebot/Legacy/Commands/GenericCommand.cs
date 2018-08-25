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
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using System;
    using System.Linq;
    using Castle.Core.Logging;

    using Helpmebot;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Model;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    ///     Generic bot command abstract class
    /// </summary>
    public abstract class GenericCommand
    {
        #region Fields

        /// <summary>
        /// The command service helper.
        /// </summary>
        protected readonly ICommandServiceHelper CommandServiceHelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="GenericCommand"/> class.
        /// </summary>
        /// <param name="commandServiceHelper">
        /// The command Service Helper.
        /// </param>
        protected GenericCommand(ICommandServiceHelper commandServiceHelper)
        {
            // FIXME: ServiceLocator - genericlogger
            this.Log = ServiceLocator.Current.GetInstance<ILogger>();

            this.CommandServiceHelper = commandServiceHelper;
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
        /// <param name="commandServiceHelper">
        /// The command Service Helper.
        /// </param>
        protected GenericCommand(
            IUser source, 
            string channel, 
            string[] args, 
            ICommandServiceHelper commandServiceHelper)
            : this(commandServiceHelper)
        {
            this.Source = source;
            this.Channel = channel;
            this.Arguments = args;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the access level of the command
        /// </summary>
        /// <value>The access level.</value>
        public LegacyUserRights AccessLevel
        {
            get
            {
                var flagAttr = (LegacyCommandFlagAttribute) this.GetType()
                    .GetCustomAttributes(typeof(LegacyCommandFlagAttribute), true)
                    .FirstOrDefault();

                if (flagAttr != null)
                {
                    return flagAttr.Level;
                }
                else
                {
                    this.Log.WarnFormat(
                        string.Format("Warning: {0} does not have an access attribute.", this.GetType()));
                    return LegacyUserRights.Developer;   
                }
            }
        }

        /// <summary>
        ///     Gets or sets the arguments.
        /// </summary>
        public string[] Arguments { get; set; }

        /// <summary>
        ///     Gets or sets the channel.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        ///     Gets or sets the source.
        /// </summary>
        public IUser Source { get; set; }

        public string Redirection { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The run command.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        public CommandResponseHandler RunCommand()
        {
            string command = this.GetType().ToString();

            this.Log.Info("Running command: " + command);

            return this.TestAccess() ? this.ReallyRunCommand() : this.OnAccessDenied();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The execute command.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected virtual CommandResponseHandler ExecuteCommand()
        {
            return new CommandResponseHandler("not implemented");
        }

        /// <summary>
        ///     Access denied to command, decide what to do
        /// </summary>
        /// <returns>A response to the command if access to the command was denied</returns>
        protected virtual CommandResponseHandler OnAccessDenied()
        {
            var response = new CommandResponseHandler();

            string message = this.CommandServiceHelper.MessageService.RetrieveMessage("AccessDenied", this.Channel, null);

            response.Respond(message, CommandResponseDestination.PrivateMessage);
            this.Log.Info("Access denied to command.");
            if (
                !AccessLog.Instance()
                     .Save(
                         new AccessLog.AccessLogEntry(
                     this.Source, 
                     this.GetType(), 
                     false, 
                     this.Channel, 
                     this.Arguments, 
                     this.AccessLevel)))
            {
                response.Respond("Error adding denied entry to access log.", CommandResponseDestination.ChannelDebug);
            }

            // don't redirect access denied messages.
            this.Redirection = null;

            return response;
        }

        /// <summary>
        ///     Access granted to command, decide what to do
        /// </summary>
        /// <returns>The response to the command</returns>
        protected virtual CommandResponseHandler ReallyRunCommand()
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
                string message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                    "AccessDeniedAccessListFailure", 
                    this.Channel, 
                    null);
                errorResponse.Respond(
                    "Error adding to access log - command aborted.", 
                    CommandResponseDestination.ChannelDebug);
                errorResponse.Respond(message, CommandResponseDestination.Default);
                return errorResponse;
            }

            this.Log.Info("Starting command execution...");
            CommandResponseHandler crh;
            try
            {
                crh = this.ExecuteCommand();
            }
            catch (Exception ex)
            {
                this.Log.Error(ex.Message, ex);
                crh = new CommandResponseHandler(ex.Message);
            }

            this.Log.Info("Command execution complete.");
            return crh;
        }

        /// <summary>
        ///     Check the access level and then decide what to do.
        /// </summary>
        /// <returns>True if the command is allowed to Execute</returns>
        protected virtual bool TestAccess()
        {
            return this.CommandServiceHelper.LegacyAccessService.IsAllowed(this.AccessLevel, this.Source);
        }

        #endregion

        protected MediaWikiSite GetLocalMediawikiSite()
        {
            var channelRepository = this.CommandServiceHelper.ChannelRepository;
            var channel = channelRepository.GetByName(this.Channel);
            
            MediaWikiSite mediaWikiSite;
            
            if (channel != null)
            {
                mediaWikiSite = this.CommandServiceHelper.MediaWikiSiteRepository.GetById(channel.BaseWiki);
            }
            else
            {
                mediaWikiSite = this.CommandServiceHelper.MediaWikiSiteRepository.GetById(1);
            }

            return mediaWikiSite;
        }
    }
}