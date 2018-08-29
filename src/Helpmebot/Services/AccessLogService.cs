// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessLogService.cs" company="Helpmebot Development Team">
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
//   Defines the AccessLogService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services
{
    using System;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Models;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;

    /// <summary>
    /// The access log service.
    /// </summary>
    public class AccessLogService : IAccessLogService
    {
        private readonly ICommandHandler commandHandler;
        private readonly ISession session;
        private readonly IFlagService flagService;
        private readonly ILogger logger;

        public AccessLogService(ICommandHandler commandHandler, ISession session, IFlagService flagService, ILogger logger)
        {
            this.commandHandler = commandHandler;
            this.session = session;
            this.flagService = flagService;
            this.logger = logger;
            
            this.logger.Info("Initialising flag access log service");
        }

        public void Start()
        {
            this.logger.Info("Starting flag access log service");
            this.commandHandler.CommandExecuted += this.OnCommandExecuted;
        }

        private void OnCommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            var entry = new FlagAccessLogEntry();
            entry.Timestamp = DateTime.Now;
            entry.Class = e.Command.GetType().FullName;
            entry.Invocation = e.Command.InvokedAs + " " + e.Command.OriginalArguments;
            entry.Nickname = e.Command.User.Nickname;
            entry.Username = e.Command.User.Username;
            entry.Hostname = e.Command.User.Hostname;
            entry.Account = e.Command.User.Account;
            entry.Context = e.Command.CommandSource;
            entry.AvailableFlags = this.flagService.GetFlagsForUser(e.Command.User, e.Command.CommandSource)
                .Aggregate(string.Empty, (s, i) => s + i);
            entry.RequiredMainCommand = e.Command.ExecutionStatus.MainFlags;
            entry.RequiredSubCommand = e.Command.ExecutionStatus.SubcommandFlags;
            entry.Result = e.Command.ExecutionStatus.AclStatus.ToString();
            
            var txn = this.session.BeginTransaction(IsolationLevel.Serializable);
            this.session.Save(entry);
            txn.Commit();
        }

        public void Stop()
        {
            this.logger.Warn("Stopping access log service");
            this.commandHandler.CommandExecuted -= this.OnCommandExecuted;
        }
    }
}
