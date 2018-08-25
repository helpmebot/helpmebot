// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TargetedFunCommand.cs" company="Helpmebot Development Team">
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
//   Defines the TargetedFunCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands.FunStuff
{
    using System.Linq;

    using Helpmebot;
    using Helpmebot.Legacy.Model;

    using helpmebot6.Commands.FunStuff;
    using Helpmebot.Legacy;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// The targeted command.
    /// </summary>
    public abstract class TargetedFunCommand : FunCommand
    {
        private string commandTarget = null;

        /// <summary>
        /// Initialises a new instance of the <see cref="TargetedFunCommand"/> class.
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
        protected TargetedFunCommand(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Gets the command target.
        /// </summary>
        protected virtual string CommandTarget
        {
            get
            {
                if (this.commandTarget == null)
                {
                    this.commandTarget = this.GetCommandTarget();
                }

                return this.commandTarget;
            }
        }

        private string GetCommandTarget()
        {
            if (this.Arguments.Any())
            {
                this.Redirection = null;
                return string.Join(" ", this.Arguments);
            }

            if (!string.IsNullOrEmpty(this.Redirection))
            {
                var commandTarget = this.Redirection;
                this.Redirection = null;
                return commandTarget;
            }

            return this.Source.Nickname;
        }

        /// <summary>
        /// Gets the target message.
        /// </summary>
        protected abstract string TargetMessage { get; }

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] messageparams = { this.CommandTarget };
            string message = this.CommandServiceHelper.MessageService.RetrieveMessage(this.TargetMessage, this.Channel, messageparams);

            return new CommandResponseHandler(message);
        }
    }
}
