// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandBase.cs" company="Helpmebot Development Team">
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
//   Defines the CommandBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The command base.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CommandBase"/> class.
        /// </summary>
        /// <param name="commandSource">
        /// The command source.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="userFlagService">
        /// The user Flag Service.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        /// <param name="accessLogService">
        /// The access Log Service.
        /// </param>
        protected CommandBase(
            string commandSource,
            IUser user,
            IEnumerable<string> arguments,
            IUserFlagService userFlagService,
            ILogger logger,
            IMessageService messageService,
            IAccessLogService accessLogService)
        {
            this.AccessLogService = accessLogService;
            this.MessageService = messageService;
            this.Logger = logger;
            this.CommandSource = commandSource;
            this.User = user;
            this.Arguments = arguments;
            this.UserFlagService = userFlagService;
        }

        /// <summary>
        /// Gets the flag.
        /// </summary>
        public abstract string Flag { get; }

        /// <summary>
        /// Gets the command source.
        /// </summary>
        public string CommandSource { get; private set; }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public IUser User { get; private set; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public IEnumerable<string> Arguments { get; private set; }

        /// <summary>
        /// Gets or sets the user flag service.
        /// </summary>
        protected IUserFlagService UserFlagService { get; set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the message service.
        /// </summary>
        protected IMessageService MessageService { get; private set; }

        /// <summary>
        /// Gets the access log service.
        /// </summary>
        protected IAccessLogService AccessLogService { get; private set; }

        /// <summary>
        /// The run.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        public IEnumerable<CommandResponse> Run()
        {
            if (this.CanExecute())
            {
                this.AccessLogService.Success(this.User, this.GetType(), this.Arguments);

                var commandResponses = this.Execute() ?? new List<CommandResponse>();
                var completedResponses = this.OnCompleted() ?? new List<CommandResponse>();

                return commandResponses.Concat(completedResponses);
            }
            
            this.AccessLogService.Failure(this.User, this.GetType(), this.Arguments);

            this.Logger.InfoFormat("Access denied for user {0}", this.User);

            return this.OnAccessDenied() ?? new List<CommandResponse>();
        }

        /// <summary>
        /// The can execute.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public virtual bool CanExecute()
        {
            return this.UserFlagService.GetFlagsForUser(this.User).Contains(this.Flag);
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected abstract IEnumerable<CommandResponse> Execute();

        /// <summary>
        /// The on access denied.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected virtual IEnumerable<CommandResponse> OnAccessDenied()
        {
            var response = new CommandResponse
                               {
                                   Destination = CommandResponseDestination.PrivateMessage,
                                   Message = this.MessageService.RetrieveMessage(Messages.OnAccessDenied, this.CommandSource, null)
                               };

            return response.ToEnumerable();
        }

        /// <summary>
        /// The on completed.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected virtual IEnumerable<CommandResponse> OnCompleted()
        {
            return null;
        }
    }
}
