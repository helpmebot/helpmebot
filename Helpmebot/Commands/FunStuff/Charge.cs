// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Charge.cs" company="Helpmebot Development Team">
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
//   Defines the Charge type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;

    using helpmebot.Commands.FunStuff;

    /// <summary>
    /// The charge.
    /// </summary>
    internal class Charge : ProtectedTargetedFunCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Charge"/> class.
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
        public Charge(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Gets the target message.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// This operation is not supported for this command
        /// </exception>
        protected override string TargetMessage
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            return this.Arguments.Length > 0
                       ? new CommandResponseHandler(new Message().get("cmdChargeParam", this.CommandTarget))
                       : new CommandResponseHandler(new Message().get("cmdCharge", new[] { this.Source.nickname }));
        }
    }
}
