// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProtectedTargetedFunCommand.cs" company="Helpmebot Development Team">
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
//   The protected targeted command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot.Commands.FunStuff
{
    using System.Linq;

    using Helpmebot;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The protected targeted command.
    /// </summary>
    public abstract class ProtectedTargetedFunCommand : TargetedFunCommand
    {
        /// <summary>
        /// The forbidden targets.
        /// </summary>
        private readonly string[] forbiddenTargets =
            {
                "itself", "himself", "herself", "themself",
                Helpmebot6.irc.Nickname.ToLower()
            };

        /// <summary>
        /// Initialises a new instance of the <see cref="ProtectedTargetedFunCommand"/> class.
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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        protected ProtectedTargetedFunCommand(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Gets the command target.
        /// </summary>
        protected override string CommandTarget
        {
            get
            {
                if (this.forbiddenTargets.Contains(base.CommandTarget))
                {
                    return this.Source.nickname;
                }

                return base.CommandTarget;
            }
        }
    }
}