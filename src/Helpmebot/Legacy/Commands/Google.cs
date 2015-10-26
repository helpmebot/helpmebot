// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Google.cs" company="Helpmebot Development Team">
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
//   Retrieves a link to block a user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Web;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    internal class Google : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Google"/> class.
        /// </summary>
        /// <param name="commandServiceHelper">
        /// The command Service Helper.
        /// </param>
        public Google(ICommandServiceHelper commandServiceHelper)
            : base(commandServiceHelper)
        {
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
        public Google(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        ///     The execute command.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            // FIXME: servicelocator
            var messageService = ServiceLocator.Current.GetInstance<IMessageService>();

            var arguments = HttpUtility.UrlEncode(string.Join(" ", this.Arguments));
            var response = messageService.RetrieveMessage("google-response", this.Channel, arguments.ToEnumerable());
            return new CommandResponseHandler(response);
        }
    }
}