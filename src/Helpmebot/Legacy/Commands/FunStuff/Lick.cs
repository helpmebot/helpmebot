﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Lick.cs" company="Helpmebot Development Team">
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
//   Defines the Lick type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot.Commands.FunStuff;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;

    /// <summary>
    /// The lick.
    /// </summary>
    internal class Lick : TargetedFunCommand
    {
        private string target = null;
        
        /// <summary>
        /// Initialises a new instance of the <see cref="Lick"/> class.
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
        public Lick(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Gets the command target.
        /// </summary>
        protected override string CommandTarget
        {
            get
            {
                if (this.target != null)
                {
                    return this.target;
                }

                if (this.Arguments.Length == 0)
                {
                    if (!string.IsNullOrWhiteSpace(this.Redirection))
                    {
                        this.target = this.Redirection;
                        this.Redirection = null;
                        return this.target;
                    }

                    return null;
                }

                this.Redirection = null;
                this.target = string.Join(" ", this.Arguments);
                return this.target;

            }
        }

        /// <summary>
        /// Gets the target message.
        /// </summary>
        protected override string TargetMessage
        {
            get
            {
                if (this.CommandTarget == null)
                {
                    return "cmdLickSelf";
                }
                
                return "cmdLick";
            }
        }
    }
}
