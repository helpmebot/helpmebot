// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Isgd.cs" company="Helpmebot Development Team">
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
//   Shortens a URL
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Globalization;
    using System.Linq;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Shortens a URL
    /// </summary>
    internal class Isgd : GenericCommand
    {
        /// <summary>
        /// The shortening service.
        /// </summary>
        private readonly IUrlShorteningService shortener;

        /// <summary>
        /// Initialises a new instance of the <see cref="Isgd"/> class.
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
        public Isgd(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
            // FIXME: remove service locator;
            this.shortener = ServiceLocator.Current.GetInstance<IUrlShorteningService>();
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The result</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length == 0)
            {
                string[] messageParameters = { "isgd", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture) };
                return new CommandResponseHandler(this.CommandServiceHelper.MessageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            // shorten the urls
            var shortUrls = this.Arguments.Select(this.shortener.Shorten);

            // construct the message
            var message = shortUrls.Implode();
            return new CommandResponseHandler(message);
        }
    }
}
