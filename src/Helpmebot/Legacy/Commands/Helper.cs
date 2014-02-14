// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.cs" company="Helpmebot Development Team">
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
//   Triggers an inter-channel alert
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Triggers an inter-channel alert
    /// </summary>
    internal class Helper : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Helper"/> class.
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
        public Helper(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            // TODO: this needs putting into its own subsystem, messageifying, configifying, etc.
            if (this.Channel == "#wikipedia-en-help")
            {
                string message = "[HELP]: " + this.Source + " needs help in #wikipedia-en-help !";
                if (this.Arguments.Length > 0)
                {
                    message += " (message: \"" + string.Join(" ", this.Arguments) + "\")";
                }

                // FIXME: ServiceLocator
                var ircClient = ServiceLocator.Current.GetInstance<IIrcClient>();
                ircClient.SendNotice("#wikipedia-en-helpers", message);
            }

            return null;
        }
    }
}